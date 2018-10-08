using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.Export
{
    public interface IExportServiceApi
    {
        [Put("/")]
        Task RequestUpdate(string questionnaireId, InterviewStatus? status, DateTime? from, DateTime? to);
    }

    public static class ExportServiceApiClientFactory
    {
        public static IExportServiceApi Create(string apikey)
        {
            var config = ConfigurationManager.AppSettings;

            var baseUrl = config[@"BaseUrl"];
            var exportService = config[@"Services.Export"];
            var originHeader = new OriginHeaderSetter(baseUrl);

            var gitHubApi = RestService.For<IExportServiceApi>($@"{exportService}/api/v1/{apikey}", new RefitSettings
            {
                HttpMessageHandlerFactory = () => originHeader
            });

            return gitHubApi;
        }
    }

    class OriginHeaderSetter : HttpClientHandler
    {
        private readonly string baseUrl;

        public OriginHeaderSetter(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add(@"Origin", baseUrl);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
