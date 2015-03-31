namespace WB.UI.Designer
{
    using System.Web;
    using System.Web.Mvc;

    public static class GlobalHelper
    {
        public const string EmptyString = "--//--";

        public const int GridPageItemsCount = 50;

        public static string CurrentAction
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["action"];
            }
        }

        public static string CurrentController
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
            }
        }

        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            
            return url.Action(action, controller, routes, HttpContext.Current.Request.Url.Scheme);
        }
    }
}