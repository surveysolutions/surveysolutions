using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Main.Core;
using WB.UI.Designer.Controllers;

namespace WB.UI.Designer
{
    using NConfig;

    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Initialization per AppDomain.
        /// </summary>
        static MvcApplication()
        {
            SetupNConfig();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            try
            {
                NcqrsInit.RebuildReadLayer();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
                #warning TLK: should not we rethrow exception here?
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;
            var currentController = string.Empty;
            var currentAction = string.Empty;
            var currRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            if (currRouteData != null)
            {
                currentController = (string)currRouteData.Values["controller"] ?? string.Empty;
                currentAction = (string)currRouteData.Values["action"] ?? string.Empty;
            }

            var ex = Server.GetLastError();

            NLog.LogManager.GetCurrentClassLogger().Error(ex);

            var controller = new ErrorController();
            var routeData = new RouteData();
            var action = "Index";

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFound";
                        break;

                    case 401:
                        action = "AccessDenied";
                        break;
                }
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        private static void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Designer.Web.config").SetAsSystemDefault();
        }

        #warning TLK: delete this when NCQRS initialization moved to Global.asax
        public static void Initialize() { }
    }
}