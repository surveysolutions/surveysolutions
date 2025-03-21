using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;

internal class AudioAuditDataExportHandler: MultimediaDataExportHandlerBase
{
    internal override MultimediaDataType MultimediaDataType { get; } = MultimediaDataType.AudioAudit;
    
    public AudioAuditDataExportHandler(IFileSystemAccessor fileSystemAccessor, 
        IOptions<ExportServiceSettings> interviewDataExportSettings, IBinaryDataSource binaryDataSource, 
        IDataExportFileAccessor dataExportFileAccessor) : base(fileSystemAccessor, interviewDataExportSettings, 
        binaryDataSource, dataExportFileAccessor)
    {
    }
}
