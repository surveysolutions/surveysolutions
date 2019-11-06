using System;
using System.Net.Http;
using System.Web;
using Autofac;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;

namespace WB.UI.Shared.Web.Kernel
{
    public class AutofacServiceLocatorFactory
    {
        public IServiceLocator GetServiceLocator()
        {
            // MVC request
            var requestLifetimeScopeFromContext = HttpContext.Current?.Items[(object)typeof(ILifetimeScope)] as ILifetimeScope;
            if (requestLifetimeScopeFromContext != null)
                return new AutofacServiceLocatorAdapter(requestLifetimeScopeFromContext);

            // WebApi v2 request
            HttpRequestMessage httpRequestMessage = HttpContext.Current?.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            if (httpRequestMessage != null)
            {
                var lifetimeScope = httpRequestMessage.GetDependencyScope().GetRequestLifetimeScope();
                if (lifetimeScope != null)
                    return new AutofacServiceLocatorAdapter(lifetimeScope);
            }

            //var requestLifetimeScope = GlobalConfiguration.Configuration.DependencyResolver.GetRequestLifetimeScope();
            //if (requestLifetimeScope != null)
            //    return new AutofacServiceLocatorAdapter(requestLifetimeScope);

            throw new ArgumentException("Can't found current scope. Supports only web request scopes.");

        }
    }
}
