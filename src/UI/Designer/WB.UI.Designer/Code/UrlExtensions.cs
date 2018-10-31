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
            var url = new Uri(HttpContext.Current.Request.Url, path);

            return url.AbsoluteUri;
        }
    }
}
