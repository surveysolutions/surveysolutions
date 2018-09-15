using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class TenantApi<T> : ITenantApi<T>, IDisposable
    {
        private static long counter = 0;
        private readonly long id;

        public TenantApi()
        {
            id = Interlocked.Increment(ref counter);
            Debug.WriteLine($"Creating new TenantApi<{typeof(T).Name}> #{id}");
        }

        public void Dispose()
        {
            Debug.WriteLine($"Disposing TenantApi<{typeof(T).Name}> #{id}");
        }

        readonly ConcurrentDictionary<TenantInfo, T> cache = new ConcurrentDictionary<TenantInfo, T>();

        public T For(TenantInfo tenant)
        {
            return cache.GetOrAdd(tenant, id =>
            {
                var httpClient = new HttpClient(new ApiKeyHandler(tenant.Id), true);
                httpClient.BaseAddress = new Uri(tenant.BaseUrl);

                return RestService.For<T>(httpClient, new RefitSettings
                {
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    },
                    HttpMessageHandlerFactory = () => new ApiKeyHandler(tenant.Id)
                });
            });
        }

        private class ApiKeyHandler : HttpClientHandler
        {
            private readonly TenantId tenantId;

            public ApiKeyHandler(TenantId tenantId)
            {
                this.tenantId = tenantId;
                
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var uri = QueryHelpers.AddQueryString(request.RequestUri.ToString(), "apiKey", this.tenantId.Id);
                request.RequestUri = new Uri(uri);
                
                var sw = Stopwatch.StartNew();
                Console.WriteLine($"Calling {request.Method}: {request.RequestUri}");
                var result = await base.SendAsync(request, cancellationToken);
                sw.Stop();
                //Console.WriteLine(
                //    $"Get {result.Content.Headers.ContentLength ?? 0}bytes in {sw.ElapsedMilliseconds}ms: {uri}");
                return result;
            }
        }
    }


}
