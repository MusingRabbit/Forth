using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RockRaiders.Util.Extensions
{
    public static class StringExtensions
    {

        /// <summary>
        /// Extends upon String.Contains() extension method to work with multiple strings
        /// E.g "Does my string contain this?".Contains("this", "my")
        /// </summary>
        /// <param name="src">
        /// String reference to be evaluated
        /// </param>
        /// <param name="strArr">
        /// The params array of strings to be checked
        /// </param>
        /// <returns>
        /// True if any value within the params array is contained within the reference string <see cref="bool"/>.
        /// </returns>
        public static bool Contains(this string src, params string[] strArr)
        {
            foreach (var t in strArr)
            {
                if (src.Contains(t))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Strips invalid filename characters from string
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string StripInvalidFilenameChars(this string src)
        {
            return Regex.Replace(src, @"[^\w\-.]+", string.Empty);
        }

        /// <summary>
        /// Stips special characters from string
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string StripSpecialChars(this string src)
        {
            return Regex.Replace(src, "[^0-9a-zA-Z:,]+", string.Empty);
        }

        /// <summary>
        /// Creates a unique MD5Hash for easy indexing
        /// </summary>
        /// <param name="src">Source string</param>
        /// <returns>Hashed string</returns>
        public static string ToMD5Hash(this string src)
        {
            var bytes = Encoding.Unicode.GetBytes(src.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);
            return hash.Aggregate(new StringBuilder(32), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
        }

        /// <summary>
        /// Returns a new string in which all the characters in the current instance, beginning
        /// at a specified position and continuing through the last position, have been deleted.
        /// If the start index is larger than the length of the string, the original string is returned.
        /// </summary>
        /// <param name="src">The string source.</param>
        /// <param name="startIndex">The zero-based position to begin deleting characters.</param>
        /// <returns>A new string that is equivalent to this string except for the removed characters if any.</returns>
        public static string TryRemove(this string src, int startIndex)
        {
            //I think String.SubString(startIndex, endIndex) might make this redundant
            if (src.Length > startIndex)
            {
                src = src.Remove(startIndex);
            }
            return src;
        }

        /// <summary>
        /// Returns a typed enum parsed from a string value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TEnum ToEnum<TEnum>(this string value)
            where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

    }
}
