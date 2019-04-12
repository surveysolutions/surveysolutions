using System;
using Microsoft.AspNetCore.Http;
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
            HttpRequest request = urlHelper.ActionContext.HttpContext.Request;
            var uri = new Uri(new Uri(request.Scheme + "://" + request.Host.Value), urlHelper.Content(path));
            return uri.ToString();
        }

        public static string AbsoluteAction(
            this IUrlHelper url,
            string actionName, 
            string controllerName, 
            object routeValues = null)
        {
            string scheme = url.ActionContext.HttpContext.Request.Scheme;
            return url.Action(actionName, controllerName, routeValues, scheme);
        }
    }
}
