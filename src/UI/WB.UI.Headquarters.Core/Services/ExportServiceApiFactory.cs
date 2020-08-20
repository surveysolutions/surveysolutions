using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Refit;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Services
{
    class ExportServiceApiFactory : IExportServiceApiFactory
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;
        private readonly IOptions<HeadquartersConfig> headquarterOptions;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;

        private readonly IHttpContextAccessor httpContextAccessor;

        public ExportServiceApiFactory(
            IOptionsSnapshot<ExportServiceConfig> exportOptions,
            IOptions<HeadquartersConfig> headquarterOptions,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            this.exportOptions = exportOptions;
            this.headquarterOptions = headquarterOptions;
            this.exportServiceSettings = exportServiceSettings;
            this.httpContextAccessor = httpContextAccessor;
        }

        public IExportServiceApi CreateClient()
        {
            if (!Uri.TryCreate(headquarterOptions.Value.BaseUrl, UriKind.RelativeOrAbsolute, out var baseUrl))
            {
                var context = httpContextAccessor.HttpContext;

                if (context != null)
                {
                    var request = context.Request;

                    baseUrl = new Uri($"{request.Scheme}://{request.Host}{"/"}");
                }
            }

            string key = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;
            
            var http = new HttpClient
            {
                BaseAddress = new Uri(exportOptions.Value.ExportServiceUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue(@"Bearer", key),

                    // TODO: Make sure that BaseUri is properly registered in HQ, with fallback to Request.Uri
                    Referrer = baseUrl
                }
            };

            http.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);

            return RestService.For<IExportServiceApi>(http);
        }
    }
}
