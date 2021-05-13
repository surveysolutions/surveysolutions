using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;

namespace WB.Infrastructure.AspNetCore
{
    public static class HttpClientFactoryExtensions
    {
        public static IHttpClientBuilder AddTransientHttpErrorsHandling(this IHttpClientBuilder http,
            TimeSpan? maximumDelay = null)
        {
            var maxDelay = maximumDelay ?? TimeSpan.FromMinutes(2);

            var delays = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromMilliseconds(200),
                retryCount: 20,
                fastFirst: true).Select(s => TimeSpan.FromTicks(Math.Min(s.Ticks, maxDelay.Ticks)));

            //https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-jittered-back-off
            return http.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(delays));
        }

        public static IHttpClientBuilder AddHttpClientWithConfigurator<TApi, TConfigurator>(
            this IServiceCollection services,
            RefitSettings? settings = null,
            Func<IServiceProvider, IServiceProvider?>? scopedServiceProvider = null)
            where TApi : class
            where TConfigurator : class, IHttpClientConfigurator<TApi>
        {
            services.AddTransient<IHttpClientConfigurator<TApi>, TConfigurator>();

            return services.AddRefitClient<TApi>(settings)
                .ConfigureHttpClient((sp, hc) =>
                {
                    var scope = scopedServiceProvider?.Invoke(sp) ?? sp;
                    var configurator = scope.GetRequiredService<IHttpClientConfigurator<TApi>>();

                    configurator.ConfigureHttpClient(hc);
                })
                .AddTransientHttpErrorsHandling();
        }
    }
}
