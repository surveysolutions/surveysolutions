using System;
using System.Web;
using System.Web.Mvc;

namespace WB.UI.Designer.Code
{
    public static class UrlExtensions
    {
        public static string ContentAbsolute(this UrlHelper urlHelper, string contentPath)
        {
            var path = urlHelper.Content(contentPath);
            return ConvertToAbsoluteUrl(path);
        }

        public static string HttpRouteUrlAbsolute(this UrlHelper urlHelper, string routeName, object routeValues)
        {
            var path = urlHelper.HttpRouteUrl(routeName, routeValues);
            return ConvertToAbsoluteUrl(path);
        }

        private static string ConvertToAbsoluteUrl(string path)
        {
            var url = new Uri(HttpContext.Current.Request.Url, path);
            return url.AbsoluteUri;
        }
    }
}
