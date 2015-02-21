using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string ToCamelCase(this string input)
        {
            if ((input == null || input.Length < 2))
                return input;

            var firstLetter = input.Substring(0, 1).ToLower();


            return firstLetter + input.Substring(1, input.Length - 1);
        }

        public static string ToPascalCase(this string input)
        {
            if ((input == null || input.Length < 2))
                return input;

            var result = input.Substring(0, 1).ToUpper() + input.Substring(1, input.Length - 1);
            return result;
        }

        public static IEnumerable<OrderRequestItem> ParseOrderRequestString(this string value)
        {
            var result = new List<OrderRequestItem>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }

            string[] list = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                var ori = new OrderRequestItem { Field = s, Direction = OrderDirection.Asc };
                if (s.EndsWith("Desc"))
                {
                    ori.Direction = OrderDirection.Desc;
                    ori.Field = s.Substring(0, s.Length - 4);
                }

                result.Add(ori);
            }

            return result;
        }

        public static bool Compare(this string source, string compareTo)
        {
            return string.Compare(source, compareTo, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool ContainsIgnoreCaseSensitive(this string source, string dest)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dest)) return false;

            return source.ToLower().Contains(dest.ToLower());
        }

        public static bool NotIn(this string source, string[] array)
        {
            return array.All(x => x.ToLower() != source.ToLower());
        }

        public static string ToWBEmailAddress(this string source)
        {
            return string.Concat("<", source, ">");
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static T Parse<T>(this string text) where T : struct
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            Type objectType = typeof(T);
            object returnValue = null;

            if (objectType == typeof(int))
            {
                int intValue;
                if (int.TryParse(text, out intValue))
                {
                    returnValue = intValue;
                }
            }

            if (objectType == typeof(decimal))
            {
                decimal decimalValue;
                if (decimal.TryParse(text, out decimalValue))
                {
                    returnValue = decimalValue;
                }
            }

            if (objectType == typeof(DateTime))
            {
                DateTime dateValue;
                if (DateTime.TryParse(text, out dateValue))
                {
                    returnValue = dateValue;
                }
            }

            if (returnValue == null)
                return default(T);

            return (T)returnValue;
        }

        public static bool IsDecimal(this string source)
        {
            decimal iSource;
            return decimal.TryParse(source, out iSource);
        }

        public static string NullIfEmptyOrWhiteSpace(this string src)
        {
            return string.IsNullOrWhiteSpace(src) ? null : src;
        }

        public static string GetDomainName(this string url)
        {
            if(string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            Uri uri;

            if(!Uri.TryCreate(url, UriKind.Absolute,  out uri))
                throw new ArgumentException("invalid url string");

            return uri.ToString().Replace(uri.PathAndQuery, "");
        }
    }
}