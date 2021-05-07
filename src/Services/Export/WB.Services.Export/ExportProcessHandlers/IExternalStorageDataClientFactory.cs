using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IExternalStorageDataClientFactory
    {
        IExternalDataClient? GetDataClient(ExternalStorageType? storageType);
    }
}
