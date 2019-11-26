using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WB.UI.Shared.Web.Extensions
{
    public static class LinkExtensions
    {
        public static IHtmlContent GenerateFavicon(this IHtmlHelper page, string faviconHref)
        {
            return new HtmlString(string.Format(
                @"<link rel='icon' type='image/png' sizes='192x192' href='{0}-192x192.png'> 
                <link rel='apple-touch-icon-precomposed' sizes='180x180' href='{0}-180x180.png'>
                <link rel='apple-touch-icon-precomposed' sizes='152x152' href='{0}-152x152.png'>
                <link rel='apple-touch-icon-precomposed' sizes='144x144' href='{0}-144x144.png'>
                <link rel='apple-touch-icon-precomposed' sizes='120x120' href='{0}-120x120.png'>
                <link rel='apple-touch-icon-precomposed' sizes='114x114' href='{0}-114x114.png'>
                <link rel='apple-touch-icon-precomposed' sizes='76x76' href='{0}-76x76.png'>
                <link rel='apple-touch-icon-precomposed' sizes='72x72' href='{0}-72x72.png'>
                <link rel='apple-touch-icon-precomposed' href='{0}.png'>", faviconHref));
        }
    }
}
