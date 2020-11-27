using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.AspNetCore;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Services
{
    class ExportServiceApiConfigurator : IHttpClientConfigurator<IExportServiceApi>
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly ILogger<ExportServiceApiConfigurator> logger;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ExportServiceApiConfigurator(
            IOptionsSnapshot<ExportServiceConfig> exportOptions,
            IOptions<HeadquartersConfig> headquarterOptions,
            
            IHttpContextAccessor httpContextAccessor,
            ILogger<ExportServiceApiConfigurator> logger, 
            IInScopeExecutor inScopeExecutor)
        {
            this.exportOptions = exportOptions;
            this.headquarterOptions = headquarterOptions;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.inScopeExecutor = inScopeExecutor;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            var valueBaseUrl = headquarterOptions.Value.BaseUrl;
            var context = httpContextAccessor.HttpContext;

            if (!Uri.TryCreate(valueBaseUrl, UriKind.Absolute, out var baseUrl))
            {
                if (context != null)
                {
                    var request = context.Request;

                    baseUrl = new Uri($"{request.Scheme}://{request.Host}/");
                }
            }
            
            this.logger.LogTrace("Using {baseUrl} as export service url. Parsed value {baseUrlUri}", valueBaseUrl, baseUrl);

            var executor = context == null
                ? inScopeExecutor
                : context.RequestServices.GetRequiredService<IInScopeExecutor>();

            string key = executor.Execute(s =>
            {
                var workspace = s.GetInstance<IWorkspaceContextAccessor>().CurrentWorkspace();

                if (workspace != null)
                {
                    hc.DefaultRequestHeaders.Add(@"x-tenant-space", workspace.Name);
                }

                IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings = s.GetInstance<IPlainKeyValueStorage<ExportServiceSettings>>();
                return exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;
            });

            hc.BaseAddress = new Uri(exportOptions.Value.ExportServiceUrl);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", key);
            hc.DefaultRequestHeaders.Referrer = baseUrl;
            hc.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);
        }
    }
}
