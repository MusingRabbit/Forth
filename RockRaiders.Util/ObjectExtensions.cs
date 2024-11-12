// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Extension Methods for managed .NET objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandyLib.Extensions
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.ComponentModel;
    using global::System.Linq;
    using global::System.Reflection;
    using RockRaiders.Util;

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
        public static T ConvertTo<T>(this object obj) where T : class, new()
        {
            var result = new T();

            result.CopyProperties(obj);

            return result;
        }

        public static CopyPropertiesAction<T, TSrc> CopyProperties<T, TSrc>(this T target, TSrc source)
            where T: class
            where TSrc : class
        {
            return new CopyPropertiesAction<T, TSrc>(target, source);
        }

        /// <summary>
        /// Copies the values of all public properties of matching type and name from one object to another.
        /// </summary>
        /// <param name="obj">LHS -> Object you wish to copy TO</param>
        /// <param name="exclArr">Properties you wish to exclude from the copy (such as an Id field)</param>
        public static void CopyProperties(this object obj, object src, params string[] exclArr)
        {
            var outProperties = TypeDescriptor.GetProperties(obj.GetType())
                .Cast<PropertyDescriptor>()
                .Where(x => !exclArr.Contains(x.Name))
                .ToDictionary(x => new { x.Name, x.PropertyType });

            var inProperties = TypeDescriptor.GetProperties(src)
                .Cast<PropertyDescriptor>()
                .Where(x => !exclArr.Contains(x.Name))
                .ToDictionary(x => new { x.Name, x.PropertyType });

            foreach (var kvp in inProperties)
            {
                if (outProperties.ContainsKey(kvp.Key) && !outProperties[kvp.Key].IsReadOnly)
                    outProperties[kvp.Key].SetValue(obj, kvp.Value.GetValue(src));
            }
        }
        
    }
}
