using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Event.Default;

namespace WB.UI.Designer.Code
{
    public static class UrlExtensions
    {
        public static string ContentAbsolute(this IHtmlHelper htmlHelper, string contentPath)
        {
            string root = "";
            if (htmlHelper.ViewData.ContainsKey("webRoot"))
            {
                root = htmlHelper.ViewData["webRoot"].ToString() ?? "";
            }
            else
            {
                var urlHelperFactory = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var actionContextAccessor = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
                root = urlHelper.Content("~")!;
            }
            return ConvertToAbsoluteUrl(root, contentPath);
        }

        private static string ConvertToAbsoluteUrl(string webRoot, string path)
        {
            if (path.StartsWith("~"))
            {
                return path.Replace("~", webRoot);
            }

            return webRoot + path;
        }
    }
}
