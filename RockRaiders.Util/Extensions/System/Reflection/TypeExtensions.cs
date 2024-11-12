using System;

namespace RockRaiders.Util
{
    public static class TypeExtensions
    {
        public static bool IsUnsignedInteger(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return true;
            }

            return false;
        }
    }
}
