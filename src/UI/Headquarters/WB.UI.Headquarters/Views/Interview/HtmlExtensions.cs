using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ASP
{
    public static partial class HtmlExtensions 
    {
        public static string InterviewItemIdWithPostfix(this HtmlHelper htmlHelper, Guid questionId, decimal[] rosterVector, string postfix = "")
        {
            return $"{questionId}_{htmlHelper.Stringify(rosterVector)}_{postfix}";
        }

        public static string Stringify(this HtmlHelper htmlHelper, decimal[] array)
        {
            return string.Join("_", array.Select(x => x.ToString("N0", CultureInfo.InvariantCulture)));
        }
    }
}