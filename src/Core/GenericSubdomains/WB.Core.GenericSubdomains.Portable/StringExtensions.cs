using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string EmptyIfNull(this string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            return input;
        }

        public static string ToCamelCase(this string input)
        {
            if (input == null || input.Length < 2)
                return input;
            
            return char.ToLower(input[0]) + input.Substring(1);
        }

        public static string ToPascalCase(this string input)
        {
            if ((input == null || input.Length < 2))
                return input;

            var result = char.ToUpper(input[0]) + input.Substring(1);
            return result;
        }

        public static int? ParseIntOrNull(this string value)
        {
            return int.TryParse(value, out int result) ? result : null as int?;
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
                if (s.EndsWith("Desc", StringComparison.OrdinalIgnoreCase))
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
            return array.All(x => !string.Equals(x, source, StringComparison.OrdinalIgnoreCase));
        }

        public static string ToWBEmailAddress(this string source)
        {
            return string.Concat("<", source, ">");
        }

        public static T Parse<T>(this string text) where T : struct
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

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

            if (objectType == typeof(double))
            {
                double doubleValue;
                if (double.TryParse(text, out doubleValue))
                {
                    returnValue = doubleValue;
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

        public static string FormatString(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        public static string PrefixEachLine(this string lines, string prefix)
            => prefix + lines.Replace(Environment.NewLine, Environment.NewLine + prefix);

        public static string RemoveControlChars(this string source)
        {
            if(!string.IsNullOrEmpty(source)) 
            {
                var chars = source.ToCharArray();
                bool containsControlChar = false;
                for(int i = 0; i < chars.Length; i++) 
                {
                    containsControlChar = Char.IsControl(chars[i]);
                    if(containsControlChar) 
                        break;
                }
                if(containsControlChar) 
                {
                    var sanitizedString = new System.Text.StringBuilder();
                    for(int i = 0; i < chars.Length; i++) 
                    {
                        if(!Char.IsControl(chars[i])) 
                        {
                            sanitizedString.Append(chars[i]);
                        }
                    }
                    return sanitizedString.ToString();
                }
            }

            return source;
        }
        
        public static bool ToBool(this string value, bool @default)
        {
            return Boolean.TryParse(value, out var result) ? result : @default;
        }

        public static int ToInt(this string value, int @default)
        {
            return Int32.TryParse(value, out var result) ? result : @default;
        }
    }
}
