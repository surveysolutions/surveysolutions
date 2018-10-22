using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
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
                var httpClient = new HttpClient(new ApiKeyHandler(tenant, logger), true);
                
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
            private readonly TenantInfo tenant;
            private readonly ILogger<TenantApi<T>> logger;

            public ApiKeyHandler(TenantInfo tenant, ILogger<TenantApi<T>> logger)
            {
                this.tenant = tenant;
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
                var uri = QueryHelpers.AddQueryString(request.RequestUri.ToString(), "apiKey", this.tenant.Id.ToString());
                request.RequestUri = new Uri(uri);

                var sw = Stopwatch.StartNew();
                logger.LogDebug($"Calling {request.Method}: {request.RequestUri}");

                var result = await this.policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
                sw.Stop();

                var size = result.Content.Headers.ContentLength ?? GetRawSizeUsingReflection(result) ?? 0;
                
                Monitoring.Http.RequestsCounter
                    .Labels(Monitoring.Http.HTTP_OUT, this.tenant.Name ?? this.tenant.Id.ToString()).Inc();

                if (size != 0)
                {
                    Monitoring.Http.DataServed
                        .Labels(Monitoring.Http.HTTP_OUT, this.tenant.Name ?? this.tenant.Id.ToString()).Inc(size);
                }

                Monitoring.Http.Latency
                    .Labels(Monitoring.Http.HTTP_OUT, this.tenant.Name ?? this.tenant.Id.ToString()).Observe(sw.Elapsed.TotalSeconds);

                logger.LogInformation($"Got {size.Bytes().Humanize("#.##")} in {sw.ElapsedMilliseconds}ms: {uri.Substring(0, 80)}...");
                return result;
            }

            private static readonly ConcurrentDictionary<Type, FieldInfo> Cache = new ConcurrentDictionary<Type, FieldInfo>();
            private readonly IAsyncPolicy<HttpResponseMessage> policy;


            private static long? GetRawSizeUsingReflection(HttpResponseMessage result)
            {
                var field = Cache.GetOrAdd(result.Content.GetType(), type =>
                {
                    var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    return type.BaseType.GetField("_originalContent", bindFlags);
                });

                var val = field.GetValue(result.Content) as HttpContent;
                return val?.Headers.ContentLength;
            }
        }
    }
}
