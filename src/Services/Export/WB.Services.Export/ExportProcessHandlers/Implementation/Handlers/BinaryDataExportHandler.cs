using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;

internal class BinaryDataExportHandler : MultimediaDataExportHandlerBase
{
    internal override MultimediaDataType MultimediaDataType { get; } = MultimediaDataType.Binary;
    public BinaryDataExportHandler(IFileSystemAccessor fileSystemAccessor, 
        IOptions<ExportServiceSettings> interviewDataExportSettings, IBinaryDataSource binaryDataSource, 
        IDataExportFileAccessor dataExportFileAccessor) : base(fileSystemAccessor, interviewDataExportSettings, 
        binaryDataSource, dataExportFileAccessor)
    {
    }
}
