﻿using System;
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
        private readonly IOptions<DataExportOptions> exportOptions;
        private readonly IOptions<HeadquarterOptions> headquarterOptions;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ExportServiceApiFactory(IOptions<DataExportOptions> exportOptions,
            IOptions<HeadquarterOptions> headquarterOptions,
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
            string key = null;

            //var localRunner = ctx.Get<ILocalExportServiceRunner>();
            //localRunner.Run();
            key = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;

            var baseUrl = headquarterOptions.Value.BaseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var context = httpContextAccessor.HttpContext;

                if (context != null)
                {
                    var request = context.Request;

                    baseUrl = $"{request.Scheme}://{request.Host}{"/"}";
                }
            }

            var http = new HttpClient
            {
                BaseAddress = new Uri(exportOptions.Value.ExportServiceUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue(@"Bearer", key),

                    // TODO: Make sure that BaseUri is properly registered in HQ, with fallback to Request.Uri
                    Referrer = new Uri(baseUrl)
                }
            };

            http.DefaultRequestHeaders.Add(@"x-tenant-name", headquarterOptions.Value.TenantName);

            return RestService.For<IExportServiceApi>(http);
        }
    }
}
