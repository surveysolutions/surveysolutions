using System.Net.Http;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExportServiceApiConfigurator
    {
        void ConfigureHttpClient(HttpClient hc);
    }
}
