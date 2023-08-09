using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Shared.Web.Services
{
    /// <summary>
    /// https://github.com/aspnet/Entropy/blob/master/samples/Mvc.RenderViewToString/RazorViewToStringRenderer.cs
    /// </summary>
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine razorViewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IServiceProvider serviceProvider;

        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            this.razorViewEngine = razorViewEngine;
            this.tempDataProvider = tempDataProvider;
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> RenderToStringAsync(string viewName, object model, string webRoot = null, 
            string webAppRoot = null, RouteData routeData = null)
        {
            using var scope = serviceProvider.CreateWorkspaceScope();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider
            };

            var actionContext = new ActionContext(httpContext, routeData ?? new RouteData(), new ActionDescriptor());

            var view = FindView(actionContext, viewName);

            if (view == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any  available view");
            }

            var viewDictionary =
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

            if (webRoot != null)
            {
                viewDictionary["webRoot"] = webRoot;
            }

            if (webAppRoot != null)
            {
                viewDictionary["webAppRoot"] = webAppRoot;
            }

            await using var sw = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                view,
                viewDictionary,
                new TempDataDictionary(
                    actionContext.HttpContext,
                    tempDataProvider),
                sw,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext);
            return sw.ToString();
        }


        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = razorViewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] {$"Unable to find view '{viewName}'. The following locations were searched:"}.Concat(
                    searchedLocations));
            ;

            throw new InvalidOperationException(errorMessage);
        }
    }
}
