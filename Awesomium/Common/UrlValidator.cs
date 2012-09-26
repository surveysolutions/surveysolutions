// -----------------------------------------------------------------------
// <copyright file="UrlValidator.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UrlValidator
    {
        private static string UrlPattern = @"^(http|https|tcp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z0-9]{1,3}(:[0-9]{1,5})?((/|\\)([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])+)*(/|\\)$";
        private static Regex RegEx = new Regex(UrlPattern);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsValid(string url)
        {
            return RegEx.IsMatch(url);
        }
    }
}
