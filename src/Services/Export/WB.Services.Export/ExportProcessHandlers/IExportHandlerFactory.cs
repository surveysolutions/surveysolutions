using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers
{
    interface IExportHandlerFactory
    {
        IExportHandler GetHandler(DataExportFormat exportFormat, ExternalStorageType? externalStorageType);
    }
}
