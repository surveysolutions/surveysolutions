using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using StackExchange.Exceptional;

namespace WB.UI.WebTester
{
    public class MvcApplication : HttpApplication
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                log.Error(args.ExceptionObject.ToString());
            };
        }

        protected void Application_End()
        {
            log.Info("Application End");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lasterror = Server.GetLastError();
            lasterror.Log(HttpContext.Current);
            log.Error(lasterror, "Application_Error");
        }
    }
}
