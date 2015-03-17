
using System.Web;
using System.Web.Http.Routing;

namespace WB.UI.Shared.Web.Extensions
{
    public static class LinkExtensions
    {
        public static IHtmlString GenerateFavicon(this System.Web.Mvc.WebViewPage page, string faviconHref)
        {
            return new HtmlString(string.Format(
                @"<link rel='icon' sizes='192x192' href='{0}'>"+
                @"<link rel='apple-touch-icon-precomposed' sizes='180x180' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='152x152' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='144x144' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='120x120' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='114x114' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='76x76' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' sizes='72x72' href='{0}'>" +
                @"<link rel='apple-touch-icon-precomposed' href='{0}'>", faviconHref));
        }         
    }
}