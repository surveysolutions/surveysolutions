#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Refit;
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

        private static readonly ConcurrentDictionary<string, IServiceProvider> providersCache = new();

        public static void AddWorkspaceAwareHttpClient<TApi, TConfigurator>(this IServiceCollection services, 
            RefitSettings? settings = null)
            where TApi: class
            where TConfigurator : class, IHttpClientConfigurator<TApi>
        {
            services.AddTransient<IHttpClientConfigurator<TApi>, TConfigurator>();

            services.AddTransient<TApi>(provider =>
            {
                var workspace = provider.GetWorkspaceContext();
                var workspaceName = workspace?.Name ?? WorkspaceConstants.DefaultWorkspaceName;
                var configurator = provider.GetRequiredService<IHttpClientConfigurator<TApi>>();
                var httpClient = providersCache.GetOrAdd(workspaceName, name =>
                {
                    var collection = new ServiceCollection();
                    collection.AddRefitClient<TApi>(settings)
                        .ConfigureHttpClient((sp, hc) =>
                        {
                            configurator.ConfigureHttpClient(hc);
                        }).AddTransientHttpErrorsHandling();

                    return collection.BuildServiceProvider();
                });

                return httpClient.GetRequiredService<TApi>();
            });

        }
    }
}
