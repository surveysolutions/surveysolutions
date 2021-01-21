using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Refit;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire.Services.Implementation;
using WB.Services.Infrastructure.Logging;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Infra
{
    public class TenantApi<T> : ITenantApi<T>
    {
        private readonly ILogger<TenantApi<T>> logger;
        private readonly IConfiguration configuration;

        public TenantApi(ILogger<TenantApi<T>> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        static readonly ConcurrentDictionary<TenantInfo, T> cache = new ConcurrentDictionary<TenantInfo, T>();

        public T For(TenantInfo? tenant)
        {
            if (tenant == null) throw new InvalidOperationException("Tenant must be not null.");

            return cache.GetOrAdd(tenant, id =>
            {
                var httpClient = new HttpClient(new ApiKeyHandler(tenant, logger), true);

                var urlOverrideKey = $"TenantUrlOverride:{tenant.ShortName}";

                if (configuration[urlOverrideKey] != null)
                {
                    var uri = configuration[urlOverrideKey];
                    
                    httpClient.BaseAddress = new Uri($"{uri.TrimEnd('/')}/{tenant.Workspace}");

                    var aspnetcoreToken = Environment.GetEnvironmentVariable("ASPNETCORE_TOKEN");
                    if (aspnetcoreToken != null)
                    {
                        httpClient.DefaultRequestHeaders.Add("MS-ASPNETCORE-TOKEN", aspnetcoreToken);
                    }
                }
                else
                {
                    httpClient.BaseAddress = new Uri(tenant.BaseUrl);
                }

                logger.LogDebug("Using tenantApi for {tenant} - {url}", tenant.Name, httpClient.BaseAddress);

                return RestService.For<T>(httpClient, new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings
                    {
                        SerializationBinder = new QuestionnaireDocumentSerializationBinder(),
                        TypeNameHandling = TypeNameHandling.Auto
                    })
                });
            });
        }

        private class ApiKeyHandler : HttpClientHandler
        {
            private readonly TenantInfo tenant;
            private readonly ILogger<TenantApi<T>> logger;
            private readonly IAsyncPolicy<HttpResponseMessage> policy;

            public ApiKeyHandler(TenantInfo tenant, ILogger<TenantApi<T>> logger)
            {
                this.tenant = tenant;
                this.logger = logger;
                this.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                this.policy = Policy
                    .HandleResult<HttpResponseMessage>(
                        message => message.RequestMessage != null && 
                                    message.RequestMessage.Method == HttpMethod.Get
                                   && !message.IsSuccessStatusCode && message.StatusCode != HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(4,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (response, timeSpan, retryCount, context) =>
                            {
                                using (LoggingHelpers.LogContext
                                    (("tenantName", tenant.Name),
                                    ("uri", response.Result.RequestMessage?.RequestUri)))
                                {
                                    logger.LogWarning("Request failed with {statusCode}. " +
                                                      "Waiting {timeSpan} before next retry. Retry attempt {retryCount}",
                                        response.Result.StatusCode, timeSpan, retryCount);
                                }
                            });
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var uri = QueryHelpers.AddQueryString(request.RequestUri!.ToString(), "apiKey", this.tenant.Id.ToString());
                request.RequestUri = new Uri(uri);

                request.Headers.Authorization = new AuthenticationHeaderValue("TenantToken", this.tenant.Id.ToString());

                using (LoggingHelpers.LogContext(("uri", request.RequestUri)))
                {
                    var sw = Stopwatch.StartNew();

                    var result = await this.policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
                    sw.Stop();

                    var size = result.Content.Headers.ContentLength ?? GetRawSizeUsingReflection(result) ?? 0;

                    Monitoring.Http.RegisterRequest(
                        this.tenant.Name ?? this.tenant.Id.ToString(),
                        sw.Elapsed.TotalSeconds,
                        size);

                    logger.LogTrace("TenantApi executed request {uri} with size {size} in {elapsed} ms",
                        request.RequestUri.LocalPath,
                        size,
                        sw.ElapsedMilliseconds);

                    return result;
                }
            }

            // ReSharper disable once StaticMemberInGenericType
            private static readonly ConcurrentDictionary<Type, FieldInfo?> Cache = new ConcurrentDictionary<Type, FieldInfo?>();

            private static long? GetRawSizeUsingReflection(HttpResponseMessage result)
            {
                var field = Cache.GetOrAdd(result.Content.GetType(), type =>
                {
                    var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    return type.BaseType?.GetField("_originalContent", bindFlags);
                });

                var val = field?.GetValue(result.Content) as HttpContent;
                return val?.Headers.ContentLength;
            }
        }
    }
}
