namespace RockRaiders.Util.Extensions
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Data;
    using global::System.Linq;
    using global::System.Reflection;
    using global::System.Text;
    using RockRaiders.Util.Attributes;

    /// <summary>
    /// The enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Takes single random element
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        /// <summary>
        /// Shuffles data and returns n number of elements
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        /// <summary>
        /// Randomly re-orders IEnumerable data
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        /// <summary>
        /// Copies the contents of an IEnumerable into a new instance of a LinkedList
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="LinkedList"/>.
        /// </returns>
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            return new LinkedList<T>(source);
        }

        /// <summary>
        /// Copies the contents of an IEnumerable into a new instance of a DataTable
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, string tableName = null)
            where T : class
        {
            var typeInfo = typeof(T);
            var result = new DataTable(tableName ?? typeInfo.Name);
            var props = typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var colAttrib = prop.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                var key = (colAttrib?.Name ?? prop.Name).ToLower();
                result.Columns.Add(key, prop.PropertyType);
            }

            foreach (var item in source)
            {
                var values = new object[props.Length];

                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                result.Rows.Add(values);
            }

            return result;
        }

        /// <summary>
        /// The to concatenated string.
        /// </summary>
        /// <param name="src">
        /// The src.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <param name="delimitor">
        /// The delimitor.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> src, Func<T, string> selector, string delimitor)
            where T : class
        {
            return ToDelimitedString(src.Select(x => selector(x)), delimitor);
        }

        /// <summary>
        /// The to concatenated string.
        /// </summary>
        /// <param name="src">
        /// The src.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <param name="delimitor">
        /// The delimitor.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> src, string delimitor)
            where T : struct
        {
            return ToDelimitedString(src.Select(x => x.ToString()), delimitor);
        }

        /// <summary>
        /// The to concatenated string.
        /// </summary>
        /// <param name="src">
        /// The src.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <param name="delimitor">
        /// The delimitor.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToDelimitedString(this IEnumerable<string> src, string delimitor)
        {
            var sb = new StringBuilder();
            var list = src.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                sb.Append(list[i] + (i == list.Count - 1 ? string.Empty : delimitor));
            }

            return sb.ToString();
        }

        public static string ToDelimitedString(this IEnumerable<string> src, char delimitor)
        {
            return ToDelimitedString(src, delimitor.ToString());
        }

        /// <summary>
        /// Generates a unique MD5 hash from an IEnumerable 
        /// </summary>
        /// <param name="src">
        /// The src.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToMD5Hash<T>(this IEnumerable<T> src, Func<T, string> selector)
            where T : class
        {
            var stringValue = $"[{src.ToDelimitedString(selector, "|")}]";
            return stringValue.ToMD5Hash();
        }
    }
}
