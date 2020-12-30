#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Refit;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Infrastructure.AspNetCore;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public static class WorkspaceExtension
    {
        public static void UseWorkspaces(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceMiddleware>();
        }

        public static void UseRedirectIntoWorkspace(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceRedirectMiddleware>();
        }

        private static readonly ConcurrentDictionary<(string, Type), object> httpClientsCache = new();
        
        public static void AddWorkspaceAwareHttpClient<TApi, TConfigurator, THandler>(
            this IServiceCollection services,
            RefitSettings? refitSettings = null)
          where TApi : class
          where THandler: DelegatingHandler
          where TConfigurator : class, IHttpClientConfigurator<TApi>
        {
            services.AddTransient<IHttpClientConfigurator<TApi>, TConfigurator>();
            services.AddTransient<THandler>();
            
            services.AddTransient<TApi>(s =>
            {
                var configurator = s.GetRequiredService<IHttpClientConfigurator<TApi>>();
                var handler = s.GetRequiredService<THandler>();
                handler.InnerHandler = new HttpClientHandler();

                var hc = new HttpClient(handler, false);

                configurator.ConfigureHttpClient(hc);

                return RestService.For<TApi>(hc, refitSettings);
            });
        }
    }
}
