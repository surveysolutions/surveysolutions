using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.Practices.ServiceLocation;
using NConfig;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Headquarters.App_Start;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static MvcApplication()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;

            var ex = Server.GetLastError();

            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            logger.Error("Unexpected error occurred", ex);

            var controller = new ErrorController();
            var routeData = new RouteData();
            var action = "Index";

            if (ex is HttpAntiForgeryException)
            {
                httpContext.Response.Redirect(httpContext.Request.Url.ToString(), true);
                return;
            }

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFound";
                        break;
                    case 403:
                        action = "Forbidden";
                        break;
                    case 401:
                        action = "AccessDenied";
                        break;
                }
            }

            var currentController = string.Empty;
            var currentAction = string.Empty;
            var currRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            if (currRouteData != null)
            {
                currentController = (string)currRouteData.Values["controller"] ?? string.Empty;
                currentAction = (string)currRouteData.Values["action"] ?? string.Empty;
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
            httpContext.Response.ContentType = "text/html";

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        /// <summary>
        /// Used to trigger execution of static constructor
        /// </summary>
        public static void Initialize()
        {
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private static bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api");
        }
    }
}