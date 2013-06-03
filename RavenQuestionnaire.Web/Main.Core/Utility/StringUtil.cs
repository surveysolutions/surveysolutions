// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringUtil.cs" company="">
//   
// </copyright>
// <summary>
//   The string util.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities;

    /// <summary>
    /// The string util.
    /// </summary>
    public static class StringUtil
    {
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
        public static string GetOrderRequestString(List<OrderRequestItem> orders)
        {
            return string.Join(
                ",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? string.Empty : "Desc")));
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

        public static bool Compare(this string source, string compareTo)
        {
            return string.Compare(source, compareTo, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool ContainsIgnoreCaseSensitive(this string source, string dest)
        {
            return source.ToLower().Contains(dest.ToLower());
        }

        public static string ToWBEmailAddress(this string source)
        {
            return string.Concat("<", source, ">");
        }

        #endregion
    }
}