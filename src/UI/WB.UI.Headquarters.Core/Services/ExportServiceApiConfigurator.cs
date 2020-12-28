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
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Services
{
    class ExportServiceApiConfigurator : IHttpClientConfigurator<IExportServiceApi>
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;

        private readonly ILogger<ExportServiceApiConfigurator> logger;
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly string exportServiceKey;

        public ExportServiceApiConfigurator(
            IOptionsSnapshot<ExportServiceConfig> exportOptions,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IOptions<HeadquartersConfig> headquarterOptions,
            IHttpContextAccessor contextAccessor,
            IWorkspaceContextAccessor workspaceContextAccessor,
            ILogger<ExportServiceApiConfigurator> logger)
        {
            this.exportOptions = exportOptions;
            this.exportServiceKey = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey)?.Key;
            this.headquarterOptions = headquarterOptions;
            this.contextAccessor = contextAccessor;
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.logger = logger;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            var valueBaseUrl = headquarterOptions.Value.BaseUrl;

            if (!Uri.TryCreate(valueBaseUrl, UriKind.Absolute, out var baseUrl))
            {
                var context = contextAccessor.HttpContext;
                if (context != null)
                {
                    var request = context.Request;

                    baseUrl = new Uri($"{request.Scheme}://{request.Host}/");
                }
            }

            this.logger.LogTrace("Using {baseUrl} as export service url. Parsed value {baseUrlUri}", valueBaseUrl, baseUrl);
            
            var workspace = workspaceContextAccessor.CurrentWorkspace();

            if (workspace != null)
            {
                hc.DefaultRequestHeaders.Add(@"x-tenant-space", workspace.Name);
            }

            hc.DefaultRequestHeaders.Referrer = baseUrl;
            hc.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", exportServiceKey);

            hc.BaseAddress = new Uri(exportOptions.Value.ExportServiceUrl);
        }
    }
}
