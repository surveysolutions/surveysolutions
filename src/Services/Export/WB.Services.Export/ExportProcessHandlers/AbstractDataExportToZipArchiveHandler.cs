﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class AbstractDataExportToZipArchiveHandler : BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        protected AbstractDataExportToZipArchiveHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.dataExportFileAccessor = dataExportFileAccessor;
        }

        public override async Task DoExportAsync(
            DataExportProcessArgs processArgs, 
            ExportSettings exportSettings, string archiveName,
            ExportProgress exportProgress, CancellationToken cancellationToken)
        {
            var tempArchivePath = this.fileSystemAccessor.CombinePath(this.ExportTempDirectoryPath, this.fileSystemAccessor.GetFileName(archiveName));

            try
            {
                using (var archiveFile = fileSystemAccessor.OpenOrCreateFile(tempArchivePath, false))
                {
                    using (var archive = dataExportFileAccessor.CreateExportArchive(archiveFile, processArgs.ArchivePassword))
                    {
                        await this.ExportDataIntoArchiveAsync(archive, exportSettings, exportProgress, cancellationToken);
                    }
                }

                fileSystemAccessor.DeleteFile(archiveName);
                fileSystemAccessor.MoveFile(tempArchivePath, archiveName);
                
                this.dataExportProcessesService.ChangeStatusType(processArgs.ProcessId, DataExportStatus.Compressing);
                exportProgress.Report(0);

                await this.dataExportFileAccessor.PublishArchiveToExternalStorageAsync(processArgs.ExportSettings.Tenant, archiveName, exportProgress);

                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                if (File.Exists(tempArchivePath)) File.Delete(tempArchivePath);
            }
        }

        protected abstract Task ExportDataIntoArchiveAsync(IZipArchive archive, ExportSettings exportSettings,
            ExportProgress exportProgress, CancellationToken cancellationToken);
    }
}
