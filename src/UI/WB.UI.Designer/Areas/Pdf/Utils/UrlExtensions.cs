using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WB.UI.Designer.Code
{
    public static class UrlExtensions
    {
        public static string ContentAbsolute(this IUrlHelper urlHelper, string contentPath)
        {
            var path = urlHelper.Content(contentPath);
            return ConvertToAbsoluteUrl(urlHelper, path);
        }

        public static string HttpRouteUrlAbsolute(this IUrlHelper urlHelper, string routeName, object routeValues)
        {
            var path = urlHelper.RouteUrl(new UrlRouteContext()
            {
                RouteName = routeName,
                Values = routeValues
            });
            return ConvertToAbsoluteUrl(urlHelper, path);
        }

        private static string ConvertToAbsoluteUrl(this IUrlHelper urlHelper, string path)
        {
            var url = new Uri(new Uri(urlHelper.ActionContext.HttpContext.Request.GetDisplayUrl()), path);
            return url.AbsoluteUri;
        }
    }
}
