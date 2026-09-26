using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Designer.Extensions
{
    internal static class AssistantProviderHttpClientExtensions
    {
        internal const string AssistantProviderHttpClientName = "AssistantProvider";

        public static IHttpClientBuilder AddAssistantProviderHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddHttpClient(AssistantProviderHttpClientName, client =>
            {
                // Keep this conservative; per-action cancellation (RequestAborted) is also applied.
                client.Timeout = TimeSpan.FromMinutes(3);

                var assistantAddress = configuration["Providers:Assistant:AssistantAddress"];
                if (!string.IsNullOrWhiteSpace(assistantAddress)
                    && Uri.TryCreate(assistantAddress, UriKind.Absolute, out var assistantUri))
                {
                    // AssistantAddress might be a full endpoint; BaseAddress is still safe (we can call absolute/relative URLs).
                    client.BaseAddress = new Uri(assistantUri.GetLeftPart(UriPartial.Authority));
                }

                var apiKey = configuration["Providers:Assistant:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Client-Api-Key", apiKey);
                }
            });
        }
    }
}

