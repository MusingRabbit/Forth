using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Converts a System.DataTable to typed IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertToEnumerable<T>(this DataTable table)
        {
            var rowList = table.Rows.Cast<DataRow>().ToList();

            foreach (var row in rowList)
            {
                yield return row.ConvertTo<T>();
            }
        }
    }
}
