using System;

namespace WB.UI.Shared.Web
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }
        public static bool IsNotNull(this object obj)
        {
            return !obj.IsNull();
        }

        public static int ToInt(this string value)
        {
            return value.ToInt(default(int));
        }
        public static byte ToByte(this string value)
        {
            return value.ToByte(default(byte));
        }
        public static bool ToBoolean(this string value)
        {
            return value.ToBoolean(false);
        }
        public static decimal ToDecimal(this string value)
        {
            return value.ToDecimal(decimal.Zero);
        }

        public static int ToInt(this string value, int @default)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            return @default;
        }
        public static byte ToByte(this string value, byte @default)
        {
            byte result;
            if (byte.TryParse(value, out result))
            {
                return result;
            }
            return @default;
        }
        public static bool ToBoolean(this string value, bool @default)
        {
            bool result;
            if (value.HasValue() && bool.TryParse(value, out result))
            {
                return result;
            }
            return @default;
        }
        public static decimal ToDecimal(this string value, decimal @default)
        {
            decimal result;
            if (value.HasValue() && decimal.TryParse(value, out result))
            {
                return result;
            }
            return @default;
        }

        public static DateTime ToDateTime(this string value, DateTime @default)
        {
            DateTime result;
            if (value.HasValue() && DateTime.TryParse(value, out result))
            {
                return result;
            }
            return @default;
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static T As<T>(this Exception source) where T: class 
        {
            if (source is T)
            {
                return source as T;
            }

            while (source.InnerException != null)
            {
                return source.InnerException.As<T>();
            }

            return null;
        }
    }
}