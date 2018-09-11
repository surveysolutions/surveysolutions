using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace WB.Services.Export.Client
{
    public interface IExportServiceApi
    {
        [Get("export")]
        Task<string> Get(string tenant);
    }

    public static class DefaultClientFactory
    {
        public static IExportServiceApi ExportServiceApiClient(string baseUri) 
            => RestService.For<IExportServiceApi>(baseUri + "api/v1/{tenant}/");
    }
}
