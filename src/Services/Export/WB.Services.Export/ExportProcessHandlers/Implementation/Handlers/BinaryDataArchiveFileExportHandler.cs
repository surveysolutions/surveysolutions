using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;

internal class BinaryDataArchiveFileExportHandler : ArchiveFileExportHandlerBase
{
    public BinaryDataArchiveFileExportHandler(IFileSystemAccessor fileSystemAccessor, 
        IOptions<ExportServiceSettings> interviewDataExportSettings, IBinaryDataSource binaryDataSource, 
        IDataExportFileAccessor dataExportFileAccessor) : base(fileSystemAccessor, interviewDataExportSettings, 
        binaryDataSource, dataExportFileAccessor, false)
    {
    }
}
