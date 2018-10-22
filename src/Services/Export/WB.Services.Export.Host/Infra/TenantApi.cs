using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Refit;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Infra
{
    public class TenantApi<T> : ITenantApi<T>, IDisposable
    {
        private readonly ILogger<TenantApi<T>> logger;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static long _counter = 0;

        private readonly long id;

        public TenantApi(ILogger<TenantApi<T>> logger)
        {
            this.logger = logger;
            id = Interlocked.Increment(ref _counter);
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
            private readonly RetryPolicy<HttpResponseMessage> policy;

            public ApiKeyHandler(TenantId tenantId, ILogger<TenantApi<T>> logger)
            {
                this.tenantId = tenantId;
                this.logger = logger;
                this.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                this.policy = Policy
                    .HandleResult<HttpResponseMessage>(
                        message => message.RequestMessage.Method == HttpMethod.Get
                                   && !message.IsSuccessStatusCode)
                    .WaitAndRetryAsync(8,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                        (response, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning($"Request failed with {response.Result.StatusCode}. " +
                                          $"Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                    });
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var uri = QueryHelpers.AddQueryString(request.RequestUri.ToString(), "apiKey", this.tenantId.Id);
                request.RequestUri = new Uri(uri);
                
                var sw = Stopwatch.StartNew();
                logger.LogDebug($"Calling {request.Method}: {request.RequestUri}");

                var result = await this.policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));

                sw.Stop();

                logger.LogTrace($"Got {result.Content.Headers.ContentLength ?? 0}bytes in {sw.ElapsedMilliseconds}ms: {uri}");
                return result;
            }
        }
    }

}
