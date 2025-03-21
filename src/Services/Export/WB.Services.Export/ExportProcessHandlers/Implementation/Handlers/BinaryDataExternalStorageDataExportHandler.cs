using Microsoft.Extensions.Options;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;

internal class BinaryDataExternalStorageDataExportHandler : ExternalStorageDataExportHandlerBase
{
    internal override MultimediaDataType MultimediaDataType { get; } = MultimediaDataType.Binary;
    public BinaryDataExternalStorageDataExportHandler(
        IOptions<ExportServiceSettings> interviewDataExportSettings,
        IExternalStorageDataClientFactory externalDataClientFactory,
        IBinaryDataSource binaryDataSource) : base(interviewDataExportSettings, externalDataClientFactory,
        binaryDataSource)
    {
    }
}
