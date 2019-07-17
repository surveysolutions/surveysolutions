using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    interface IExportHandlerFactory
    {
        IExportHandler GetHandler(DataExportFormat exportFormat, ExternalStorageType? externalStorageType);
    }
}
