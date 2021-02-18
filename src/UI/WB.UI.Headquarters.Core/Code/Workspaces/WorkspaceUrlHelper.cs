#nullable enable

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace WB.UI.Headquarters.Code.Workspaces
{
    // public class WorkspaceUrlHelper : UrlHelper
    // {
    //     public WorkspaceUrlHelper(ActionContext actionContext)
    //         : base(actionContext) { }
    //
    //     protected override string GenerateUrl(string protocol, string host, VirtualPathData pathData, string fragment)
    //     {
    //         return base.GenerateUrl(protocol, host, pathData, fragment);
    //     }
    //
    //     public override string Action(UrlActionContext actionContext)
    //     {
    //         return base.Action(actionContext);
    //         //actionContext.Controller.
    //         // var controller = actionContext.Controller;
    //         // var action = actionContext.Action;
    //         // return $"You wrote {controller} > {action}.";
    //     }    
    // }

    // public class WorkspaceUrlHelperFactory : IUrlHelperFactory
    // {
    //     public IUrlHelper GetUrlHelper(ActionContext context)
    //     {
    //         return new WorkspaceUrlHelper(context);
    //     }
    // }
    
    public class WorkspaceUrlHelper : UrlHelperBase
    {
        private readonly IUrlHelper urlHelper;

        public WorkspaceUrlHelper(ActionContext actionContext, IUrlHelper urlHelper)
            :base(actionContext)
        {
            //this.ActionContext = actionContext;
            this.urlHelper = urlHelper;
        }

        public override string? Action(UrlActionContext actionContext)
        {
            if (actionContext.Controller != null)
            {
                var assembly = Assembly.GetAssembly(typeof(WorkspaceUrlHelper));
                var controllerType = assembly!.GetTypes().FirstOrDefault(t => t.Name == actionContext.Controller);
                var attribute = controllerType?.GetCustomAttribute<WorkspaceAttribute>();
                if (attribute != null)
                {
                    var workspaceName = attribute.WorkspaceName;
                    if (actionContext.Values == null)
                    {
                        actionContext.Values = GetValuesDictionary(actionContext.Values);

                    }
                }
                
            }
            return urlHelper.Action(actionContext);
        }

        public override string? RouteUrl(UrlRouteContext routeContext)
        {
            return urlHelper.RouteUrl(routeContext);
        }

        /*public string? Content(string? contentPath)
        {
            return urlHelper.Content(contentPath);
        }

        public bool IsLocalUrl(string? url)
        {
            return urlHelper.IsLocalUrl(url);
        }

        public string? RouteUrl(UrlRouteContext routeContext)
        {
            return urlHelper.RouteUrl(routeContext);
        }

        public string? Link(string? routeName, object? values)
        {
            return urlHelper.Link(routeName, values);
        }

        public ActionContext ActionContext { get; }*/
    }
    
    public class WorkspaceUrlHelperFactory : IUrlHelperFactory
    {
        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var urlHelper = new UrlHelperFactory().GetUrlHelper(context);
            if (urlHelper is WorkspaceUrlHelper)
                return urlHelper;

            var workspaceUrlHelper = new WorkspaceUrlHelper(context, urlHelper);
            context.HttpContext.Items[typeof(IUrlHelper)] = workspaceUrlHelper;
            return workspaceUrlHelper;
        }
    }

    /*public class WorkspaceLinkGenerator : LinkGenerator
    {
        
    }*/
}