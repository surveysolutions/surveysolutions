using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;

namespace WB.UI.Headquarters.Services
{
    public static class DataExportClientExtensions
    {
        private static IAsyncPolicy<HttpResponseMessage> DataExportRetryJitter(this PolicyBuilder<HttpResponseMessage> policy)
        {
            Random jitterer = new Random();
            return policy
                .WaitAndRetryAsync(6,    // exponential back-off plus some jitter
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))
                );
        }

        public static void AddDataExportClient(this IServiceCollection services)
        {
            services.AddRefitClient<IExportServiceApi>()
                .ConfigureHttpClient((sp, hc) =>
                {
                    var factory = sp.GetService<IExportServiceApiConfigurator>();
                    factory.ConfigureHttpClient(hc);
                }).AddTransientHttpErrorPolicy(c => c.DataExportRetryJitter());
        }
    }
}
