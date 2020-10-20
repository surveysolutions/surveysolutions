using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.AspNetCore;

namespace WB.UI.Headquarters.Services
{
    class ExportServiceApiConfigurator : IHttpClientConfigurator<IExportServiceApi>
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private ILogger<ExportServiceApiConfigurator> logger;

        private readonly IHttpContextAccessor httpContextAccessor;

        public ExportServiceApiConfigurator(
            IOptionsSnapshot<ExportServiceConfig> exportOptions,
            IOptions<HeadquartersConfig> headquarterOptions,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ExportServiceApiConfigurator> logger)
        {
            this.exportOptions = exportOptions;
            this.headquarterOptions = headquarterOptions;
            this.exportServiceSettings = exportServiceSettings;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            var valueBaseUrl = headquarterOptions.Value.BaseUrl;
            this.logger.LogDebug("Using {baseUrl} as export service url", valueBaseUrl);
            if (!Uri.TryCreate(valueBaseUrl, UriKind.Absolute, out var baseUrl))
            {
                var context = httpContextAccessor.HttpContext;

                if (context != null)
                {
                    var request = context.Request;

                    baseUrl = new Uri($"{request.Scheme}://{request.Host}/");
                }
            }

            string key = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;
            hc.BaseAddress = new Uri(exportOptions.Value.ExportServiceUrl);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", key);
            hc.DefaultRequestHeaders.Referrer = baseUrl;
            hc.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);
        }
    }
}
