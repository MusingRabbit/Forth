using RockRaiders.Util.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace RockRaiders.Util.Extensions
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// Converts a data row to object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this DataRow row)
        {
            var result = Activator.CreateInstance<T>();
            var table = row.Table;
            var columnList = table.Columns.Cast<DataColumn>().ToList();
            var type = typeof(T);
            var propertyDict = new Dictionary<string, PropertyInfo>();

            //Itterate through each property within type <T> to create a property to column map
            foreach (var prop in type.GetProperties())
            {
                var colAttrib = prop.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                var key = (colAttrib?.Name ?? prop.Name).ToLower();
                propertyDict.Add(key, prop);
            }

            //Itterate though each column in data row to map column values to result object property
            foreach (var col in columnList)
            {
                var key = col.ColumnName.ToLower();

                if (propertyDict.ContainsKey(key))
                {
                    var propInfo = propertyDict[key];

                    if (propInfo.PropertyType.IsAssignableFrom(col.DataType))
                    {
                        var value = row[col];

                        if (value is DBNull)
                        {
                            continue;
                        }

                        propInfo.SetValue(result, row[col]);
                    }
                    else if(col.DataType.IsUnsignedInteger())
                    {
                        var value = row[col];

                        if (value is DBNull)
                        {
                            continue;
                        }

                        propInfo.SetValue(result, Convert.ToInt32(row[col]));
                    }
                }
            }


            return result;
        }
    }
}
