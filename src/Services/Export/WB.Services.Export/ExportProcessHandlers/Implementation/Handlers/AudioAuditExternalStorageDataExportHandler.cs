using Microsoft.Extensions.Options;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;

internal class AudioAuditExternalStorageDataExportHandler: ExternalStorageDataExportHandlerBase
{
    internal override MultimediaDataType MultimediaDataType { get; } = MultimediaDataType.AudioAudit;
    public AudioAuditExternalStorageDataExportHandler(
        IOptions<ExportServiceSettings> interviewDataExportSettings,
        IExternalStorageDataClientFactory externalDataClientFactory,
        IBinaryDataSource binaryDataSource) : base(interviewDataExportSettings, externalDataClientFactory,
        binaryDataSource)
    {
    }
    
}
