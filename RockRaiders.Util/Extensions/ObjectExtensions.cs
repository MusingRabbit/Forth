using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

using RockRaiders.Util.Helpers;

namespace RockRaiders.Util.Extensions
{

    /// <summary>
    /// The object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Creates a new instance of specified type, performing a shallow copy to pre-populate data.
        /// </summary>
        /// <typeparam name="T">Specified output type</typeparam>
        /// <param name="obj">lhs/source object</param>
        /// <returns>
        /// new T() with properties of source object.
        /// </returns>
        public static T CopyTo<T>(this object obj) where T : class, new()
        {
            var result = new T();

            result.CopyProperties(obj);

            return result;
        }

        /// <summary>
        /// (Hack) Iterates through every string property of the specified object and sets all string properties whose value is null to an empty string
        /// </summary>
        /// <param name="src"></param>
        public static void EmptyNullStringProperties(this object src)
        {
            var type = src.GetType();
            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var val = (string)prop.GetValue(src);

                    if (val == null)
                    {
                        prop.SetValue(src, "");
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the target object into a memory stream and returns its length. 
        /// All references, and child references must be marked as [Serializable] for this to work.
        /// (Hack) to get around the fact that Marshall.SizeOf will not work for managed objects
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static long GetByteSize(this object src)
        {
            var b = new BinaryFormatter();
            var m = new MemoryStream();
            b.Serialize(m, src);
            return m.Length;
        }

        public static long GetByteSizeExt(this object src)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            byte[] Array;
            bf.Serialize(ms, src);
            Array = ms.ToArray();
            return Array.Length;
        }


        /// <summary>
        /// A try/catch wrapper for Convert.ChangeType.
        /// Works much like TryParse
        /// </summary>
        /// <typeparam name="TOut">The value type to be converted to</typeparam>
        /// <param name="src">The object to be converts</param>
        /// <param name="result">A new instance of <see cref="TOut"/></param>
        /// <returns></returns>
        public static bool TryConvertTo<TOut>(this object src, out TOut result)
            where TOut : IComparable
        {
            result = Activator.CreateInstance<TOut>();

            try
            {
                result = (TOut)Convert.ChangeType(src, typeof(TOut));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a CopyProperties action for performing a shallow copy from one object to another
        /// </summary>
        /// <typeparam name="T">Target property type</typeparam>
        /// <typeparam name="TSrc">Source property type</typeparam>
        /// <param name="target">Target property reference</param>
        /// <param name="source">Source property reference</param>
        /// <returns></returns>
        public static CopyPropertiesAction<T, TSrc> CopyProperties<T, TSrc>(this T target, TSrc source)
            where T : class
            where TSrc : class
        {
            if (target == null)
            {
                throw new ArgumentException("'target' is null");
            }

            if (source == null)
            {
                throw new ArgumentException("'source' is null");
            }

            return new CopyPropertiesAction<T, TSrc>(target, source);
        }

        /// <summary>
        /// Creates a <see cref="XmlDocument"/>for specified type/>
        /// </summary>
        /// <typeparam name="T">The type to be converted into an XMLDocument</typeparam>
        /// <param name="src">Source reference</param>
        /// <returns></returns>
        public static XmlDocument ToXmlDocument<T>(this T src) where T : class
        {
            return XMLHelper.SerializeToXMLDocument(src);
        }

        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    var clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        public static T DeepCopy<T>(this T original)
            where T : class
        {
            return (T)InternalCopy(original, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }


        /// <summary>
        /// Serialize a target object into a dataset.
        /// </summary>
        /// <typeparam name="T">Generic class.</typeparam>
        /// <param name="objectToSerialize">Object to be serialized</param>
        /// <returns>Returns a generic type.</returns>
        public static string SerializeInDataSet<T>(this T objectToSerialize) where T : class
        {
            DataTable dataTable = objectToSerialize.ConvertToDataTable(new List<string>() { "E_ID" });
            var newDataSet = new DataSet();
            newDataSet.Tables.Add(dataTable);

            using (var textWriter = new StringWriter())
            {
                newDataSet.Tables[0].WriteXml(textWriter, XmlWriteMode.WriteSchema);
                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Convert an object to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">Generic object type.</typeparam>
        /// <param name="objectToDataTable">Object to be converted.</param>
        /// <param name="exclusions">List of column names to be excluded from DataTable.</param>
        /// <param name="tableName">Optional Table name.</param>
        /// <returns>A <see cref="DataTable"/></returns>
        public static DataTable ConvertToDataTable<T>(this T objectToDataTable, List<string> exclusions, string tableName = "Table")
        {
            var properties = typeof(T).GetProperties();

            var retVal = new DataTable(tableName);

            foreach (var info in properties)
            {
                if (exclusions.Contains(info.Name))
                {
                    // Skip adding the column if it is included in the exclusion list.
                    continue;
                }

                if (info.PropertyType.IsEnum)
                {
                    // Add enum column as integer.
                    retVal.Columns.Add(new DataColumn(info.Name, typeof(int)));
                }
                else
                {
                    // Add columns stripping of nullable attribute.
                    retVal.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
                }
            }

            var row = retVal.NewRow();
            foreach (var property in properties)
                if (!exclusions.Contains(property.Name))
                {
                    // Adds concurrent values.
                    row[property.Name] = property.GetValue(objectToDataTable) ?? DBNull.Value;
                }

            retVal.Rows.Add(row);
            return retVal;
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }
}

