using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IExternalStorageDataClientFactory
    {
        IExternalDataClient? GetDataClient(ExternalStorageType? storageType);
    }
}
