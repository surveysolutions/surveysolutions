using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class TenantApi<T> : ITenantApi<T>, IDisposable
    {
        private readonly ILogger<TenantApi<T>> logger;
        private static long counter = 0;
        private readonly long id;

        public TenantApi(ILogger<TenantApi<T>> logger)
        {
            this.logger = logger;
            id = Interlocked.Increment(ref counter);
            logger.LogTrace($"Creating new TenantApi<{typeof(T).Name}> #{id}");
        }

        public void Dispose()
        {
            logger.LogTrace($"Disposing TenantApi<{typeof(T).Name}> #{id}");
        }

        readonly ConcurrentDictionary<TenantInfo, T> cache = new ConcurrentDictionary<TenantInfo, T>();

        public T For(TenantInfo tenant)
        {
            return cache.GetOrAdd(tenant, id =>
            {
                var httpClient = new HttpClient(new ApiKeyHandler(tenant.Id, logger), true);
                
                httpClient.BaseAddress = new Uri(tenant.BaseUrl);

                return RestService.For<T>(httpClient, new RefitSettings
                {
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    }
                });
            });
        }

        private class ApiKeyHandler : HttpClientHandler
        {
            private readonly TenantId tenantId;
            private readonly ILogger<TenantApi<T>> logger;

            public ApiKeyHandler(TenantId tenantId, ILogger<TenantApi<T>> logger)
            {
                this.tenantId = tenantId;
                this.logger = logger;
                this.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var uri = QueryHelpers.AddQueryString(request.RequestUri.ToString(), "apiKey", this.tenantId.Id);
                logger.LogTrace("Appending apiKey: " + this.tenantId.Id);
                request.RequestUri = new Uri(uri);
                
                var sw = Stopwatch.StartNew();
                logger.LogTrace($"Calling {request.Method}: {request.RequestUri}");

                var result = await base.SendAsync(request, cancellationToken);

                sw.Stop();

                logger.LogTrace($"Got {result.Content.Headers.ContentLength ?? 0}bytes in {sw.ElapsedMilliseconds}ms: {uri}");
                return result;
            }
        }
    }


}
