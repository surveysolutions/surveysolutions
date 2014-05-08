using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities;
using Newtonsoft.Json;

namespace Main.Core.Utility
{
    /// <summary>
    /// The string util.
    /// </summary>
    public static class StringUtil
    {
        public static string GetOrderRequestString(IEnumerable<OrderRequestItem> orders)
        {
            return orders == null ? string.Empty : string.Join(
                ",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? string.Empty : " Desc")));
        }

        public static string GetOrderRequestString(List<OrderRequestItem> orders)
        {
            return string.Join(
                ",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? string.Empty : " Desc")));
        }

        public static List<OrderRequestItem> ParseOrderRequestString(string value)
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

        public static T DeserializeJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
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

        public static T Parse<T>(this string text) where T:struct
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

            if (objectType == typeof (decimal))
            {
                decimal decimalValue;
                if (decimal.TryParse(text, out decimalValue))
                {
                    returnValue = decimalValue;
                }
            }

            if (objectType == typeof (DateTime))
            {
                DateTime dateValue;
                if (DateTime.TryParse(text, out dateValue))
                {
                    returnValue = dateValue;
                }
            }

            if (returnValue == null)
                return default(T);
            
            return (T) returnValue;
        }
    }
}