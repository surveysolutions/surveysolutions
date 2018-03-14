using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;

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
            return page.Raw(Encoder.JavaScriptEncode(sourceMessage, false));
        }
    }
}
