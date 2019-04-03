namespace WB.UI.Designer
{
    using System.Web;
    using System.Web.Mvc;

    public static class GlobalHelper
    {
    

        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            
            return url.Action(action, controller, routes, HttpContext.Current.Request.Url.Scheme);
        }
    }
}
