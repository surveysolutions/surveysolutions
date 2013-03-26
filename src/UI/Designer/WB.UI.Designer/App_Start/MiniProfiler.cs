using System.Configuration;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;
using System.Linq;
using System.Web;
using System.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(
	typeof(WB.UI.Designer.App_Start.MiniProfilerPackage), "PreStart")]

[assembly: WebActivator.PostApplicationStartMethod(
	typeof(WB.UI.Designer.App_Start.MiniProfilerPackage), "PostStart")]


namespace WB.UI.Designer.App_Start 
{
    public static class MiniProfilerPackage
    {
        public static bool IsEnabled()
        {
            #warning TLK: deal with NConfig initialization order (should already be initialized here) and get rid of this line
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            bool isMiniProfilerEnabled;
            return bool.TryParse(ConfigurationManager.AppSettings["EnableMiniProfiler"], out isMiniProfilerEnabled)
                ? isMiniProfilerEnabled
                : false;
        }

        public static void PreStart()
        {
            if (!IsEnabled())
                return;

            // Be sure to restart you ASP.NET Developement server, this code will not run until you do that. 

            //TODO: See - _MINIPROFILER UPDATED Layout.cshtml
            //      For profiling to display in the UI you will have to include the line @StackExchange.Profiling.MiniProfiler.RenderIncludes() 
            //      in your master layout

            //TODO: Non SQL Server based installs can use other formatters like: new StackExchange.Profiling.SqlFormatters.InlineFormatter()
            MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

			//TODO: To profile a standard DbConnection: 
			// var profiled = new ProfiledDbConnection(cnn, MiniProfiler.Current);

            //TODO: If you are profiling EF code first try: 
			// MiniProfilerEF.Initialize();

            //Make sure the MiniProfiler handles BeginRequest and EndRequest
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));

            //Setup profiler for Controllers via a Global ActionFilter
            GlobalFilters.Filters.Add(new ProfilingActionFilter());

			// You can use this to check if a request is allowed to view results
            //MiniProfiler.Settings.Results_Authorize = (request) =>
            //{
                // you should implement this if you need to restrict visibility of profiling on a per request basis 
            //    return !DisableProfilingResults; 
            //};

            // the list of all sessions in the store is restricted by default, you must return true to alllow it
            //MiniProfiler.Settings.Results_List_Authorize = (request) =>
            //{
                // you may implement this if you need to restrict visibility of profiling lists on a per request basis 
                //return true; // all requests are kosher
            //};
        }

        public static void PostStart()
        {
            if (!IsEnabled())
                return;

            // Intercept ViewEngines to profile all partial views and regular views.
            // If you prefer to insert your profiling blocks manually you can comment this out
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (var item in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            if (!MiniProfilerPackage.IsEnabled())
                return;

            context.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Request;
                //TODO: By default only local requests are profiled, optionally you can set it up
                //  so authenticated users are always profiled
                if (request.IsLocal) { MiniProfiler.Start(); }
            };


            // TODO: You can control who sees the profiling information
            /*
            context.AuthenticateRequest += (sender, e) =>
            {
                if (!CurrentUserIsAllowedToSeeProfiler())
                {
                    StackExchange.Profiling.MiniProfiler.Stop(discardResults: true);
                }
            };
            */

            context.EndRequest += (sender, e) =>
            {
                MiniProfiler.Stop();
            };
        }

        public void Dispose() { }
    }
}

