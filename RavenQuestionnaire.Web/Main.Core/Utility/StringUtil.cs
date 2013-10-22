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
        private const string SubstitutionVariableDelimiter = "%";
        private static readonly string AllowedSubstitutionVariableNameRegexp = string.Format(@"(?<={0})(\w+(?={0}))", SubstitutionVariableDelimiter);
        public const string DefaultSubstitutionText = "[...]";

        #region Public Methods and Operators

        /// <summary>
        /// The get order request string.
        /// </summary>
        /// <param name="orders">
        /// The orders.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetOrderRequestString(IEnumerable<OrderRequestItem> orders)
        {
            return orders == null ? string.Empty : string.Join(
                ",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? string.Empty : " Desc")));
        }

        /// <summary>
        /// The get order request string.
        /// </summary>
        /// <param name="orders">
        /// The orders.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetOrderRequestString(List<OrderRequestItem> orders)
        {
            return string.Join(
                ",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? string.Empty : " Desc")));
        }

        /// <summary>
        /// The parse order request string.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; Main.Core.Entities.OrderRequestItem].
        /// </returns>
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

        /// <summary>
        /// The compare.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="compareTo">
        /// The compare to.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool Compare(this string source, string compareTo)
        {
            return string.Compare(source, compareTo, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// The contains ignore case sensitive.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ContainsIgnoreCaseSensitive(this string source, string dest)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dest)) return false;

            return source.ToLower().Contains(dest.ToLower());
        }

        /// <summary>
        /// The not in.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool NotIn(this string source, string[] array)
        {
            return array.All(x => x.ToLower() != source.ToLower());
        }

        /// <summary>
        /// The deserialize json.
        /// </summary>
        /// <param name="json">
        /// The json.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
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

        //move to specific class dedicated for substitution
        public static string[] GetAllSubstitutionVariableNames(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return new string[0];

            var allOccurenses = Regex.Matches(source, AllowedSubstitutionVariableNameRegexp).OfType<Match>().Select(m => m.Value).Distinct();
            return allOccurenses.ToArray();
        }

        public static string ReplaceSubstitutionVariable(this string text, string variable, string replaceTo)
        {
            return text.Replace(string.Format("{1}{0}{1}", variable, SubstitutionVariableDelimiter), replaceTo);
        }

        public static T Parse<T>(this string text) where T:struct
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            Type objectType = typeof(T);
            object returnValue = null;

            if (objectType == typeof(int))
            {
                returnValue = int.Parse(text, CultureInfo.InvariantCulture);
            }

            if (objectType == typeof (decimal))
            {
                returnValue = decimal.Parse(text, CultureInfo.InvariantCulture);
            }

            if (objectType == typeof (DateTime))
            {
                returnValue = DateTime.Parse(text, CultureInfo.InvariantCulture);
            }

            return (T) returnValue;
        }

        #endregion
    }
}