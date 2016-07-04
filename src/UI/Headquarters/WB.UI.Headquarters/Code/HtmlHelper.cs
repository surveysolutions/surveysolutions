using System.Web;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Code
{
    public static class HtmlHelper
    {
        public static IHtmlString ToSafeJavascriptMessage(this HtmlHelper<dynamic> page, string sourceMessage)
        {
            return page.Raw(sourceMessage.Replace(@"""", @"'"));
        }
    }
}