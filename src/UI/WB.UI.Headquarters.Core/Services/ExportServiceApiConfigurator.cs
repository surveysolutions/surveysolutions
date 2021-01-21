using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
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
    class ExportServiceApiHttpHandler : DelegatingHandler
    {
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly IHttpContextAccessor contextAccessor;

        public ExportServiceApiHttpHandler(
            IOptions<HeadquartersConfig> headquarterOptions,
            IHttpContextAccessor contextAccessor)
        {
            this.headquarterOptions = headquarterOptions;
            this.contextAccessor = contextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var valueBaseUrl = headquarterOptions.Value.BaseUrl;

            if (!Uri.TryCreate(valueBaseUrl, UriKind.Absolute, out var baseUrl))
            {
                var context = contextAccessor.HttpContext;
                if (context != null)
                {
                    var httpRequest = context.Request;

                    baseUrl = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}/");
                }
            }
            
            request.Headers.Referrer = baseUrl;


            return base.SendAsync(request, cancellationToken);
        }

    }

    class ExportServiceApiConfigurator : IHttpClientConfigurator<IExportServiceApi>
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly string exportServiceKey;

        public ExportServiceApiConfigurator(
            IOptionsSnapshot<ExportServiceConfig> exportOptions,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IWorkspaceContextAccessor workspaceContextAccessor,
            IOptions<HeadquartersConfig> headquarterOptions)
        {
            this.exportOptions = exportOptions;
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.exportServiceKey = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey)?.Key;
            this.headquarterOptions = headquarterOptions;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            hc.BaseAddress = new Uri(this.exportOptions.Value.ExportServiceUrl);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", exportServiceKey);
            hc.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);

            var workspace = workspaceContextAccessor.CurrentWorkspace();

            if (workspace != null)
            {
                hc.DefaultRequestHeaders.Add(@"x-tenant-space", workspace.Name);
            }
        }
    }
    
}
