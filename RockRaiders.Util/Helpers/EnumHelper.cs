using System.ComponentModel;
using System.Linq;

namespace System
{
    public static class EnumHelper
    {
        public static T Parse<T>(string value)
            where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException($"T ({type.Name}) is not an enumerated type");
            }

            return (T)Enum.Parse(type, value);
        }

        public static string GetDescription(this Enum value)
        {
            return GetDescription(value);
        }

        public static string GetDescription<T>(object value)
        {
            if (value == null)
            {
                throw new ArgumentException("value");
            }

            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new InvalidOperationException($"{type.Name} is not an Enum");
            }

            var fieldInfo = type.GetField(value.ToString());
            var description = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            var result = description?.Description ?? fieldInfo.Name;
            return result;
        }


    }
}
