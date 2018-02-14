using System.Web;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Code
{
    public static class HtmlHelperExtentions
    {
        public static IHtmlString ToSafeJavascriptMessage(this HtmlHelper<dynamic> page, string sourceMessage)
        {
            return ToSafeJavascriptMessage(page as HtmlHelper, sourceMessage);
        }

        public static IHtmlString ToSafeJavascriptMessage(this HtmlHelper page, string sourceMessage)
        {
            return page.Raw(sourceMessage.Replace(@"""", @"'"));
        }
    }
}