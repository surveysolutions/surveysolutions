﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
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
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor, IBinaryDataSource binaryDataSource)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override async Task ExportDataIntoArchiveAsync(IZipArchive archive, ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            await binaryDataSource.ForEachInterviewMultimediaAsync(settings,  
                data =>
                {
                    var path = this.fileSystemAccessor.CombinePath(data.InterviewId.FormatGuid(), data.FileName);
                    archive.CreateEntry(path, data.Content);
                    return Task.CompletedTask;
                },
                audioAuditRecord =>
                {
                    var recordingFolder = this.fileSystemAccessor.CombinePath(audioAuditRecord.InterviewId.FormatGuid(), "Recording");
                    var filePath = this.fileSystemAccessor.CombinePath(recordingFolder, audioAuditRecord.FileName);
                    archive.CreateEntry(filePath, audioAuditRecord.Content);
                    return Task.CompletedTask;
                },
                progress, 
                cancellationToken);
        }
    }
}
