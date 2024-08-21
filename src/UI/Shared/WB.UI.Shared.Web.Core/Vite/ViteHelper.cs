using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if DEBUG
using Yarp.ReverseProxy.Forwarder;
#endif

namespace Vite.Extensions.AspNetCore;

/// <remarks>See Github sources from <see href="https://github.com/alfeg/vite-aspnetcore">Source</see></remarks>
public static class ViteHelper
{
    public static IServiceCollection AddViteHelper(this IServiceCollection services, Action<ViteTagOptions>? options = null)
    {
        services.AddOptions<ViteTagOptions>()
            .PostConfigure(c =>
            {
                options?.Invoke(c);
            });
        
        //services.AddTransient<ITagHelperComponent, ViteTagHelperComponent>();
        services.AddMemoryCache();

        #if DEBUG
        services.AddHttpForwarder();
        #endif
        return services;
    }

    [Conditional("DEBUG")]
    public static void UseViteForwarder(this IApplicationBuilder app)
    {
#if DEBUG
        // Configure our own HttpMessageInvoker for outbound calls for proxy operations
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            EnableMultipleHttp2Connections = true,
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,            
        });

        // Proxy /.vite requests to Vite running on 3000 port
        app.Map("/.vite", vite =>
        {
            vite.Run(async (ctx) =>
            {
                var forwarder = ctx.RequestServices.GetRequiredService<IHttpForwarder>();
                var config = ctx.RequestServices.GetRequiredService<IOptions<ViteTagOptions>>();
                await forwarder.SendAsync(ctx, $"http://localhost:{config.Value.VitePort}/.vite/", httpClient);
            });
        });
#endif
    }
}
