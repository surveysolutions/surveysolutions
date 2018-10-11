using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class BinaryFormatDataExportHandler : AbstractDataExportToZipArchiveHandler
    {
        private readonly IBinaryDataSource binaryDataSource;
        
        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor, IBinaryDataSource binaryDataSource)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override async Task ExportDataIntoArchive(IZipArchive archive, ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            await binaryDataSource.ForEachMultimediaAnswerAsync(settings,  data =>
            {
                var path = this.fileSystemAccessor.CombinePath(data.InterviewId.FormatGuid(), data.Answer);
                archive.CreateEntry(path, data.Content);
                return Task.CompletedTask;
            }, progress, cancellationToken);
        }
    }
}
