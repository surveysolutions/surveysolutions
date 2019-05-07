using Microsoft.AspNetCore.Html;

namespace WB.UI.Designer.Utils
{
    public static class Pdf
    {
        public static HtmlString Format(string stringWithFormat, params  object[] args)
        {
            return new HtmlString(string.Format(stringWithFormat, args));
        }

        public static string WrapWith(this int number, string tag)
        {
            return $"<{tag}>{number}</{tag}>";
        }
    }
}
