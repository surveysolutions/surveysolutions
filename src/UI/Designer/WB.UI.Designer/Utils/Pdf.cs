using System.Web.Mvc;

namespace WB.UI.Designer.Utils
{
    public static class Pdf
    {
        public static MvcHtmlString Format(string stringWithFormat, params  object[] args)
        {
            return MvcHtmlString.Create(string.Format(stringWithFormat, args));
        }

        public static string WrapWith(this int number, string tag)
        {
            return $"<{tag}>{number}</{tag}>";
        }
    }
}