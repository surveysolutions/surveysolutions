﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal abstract class AbstractExternalStorageDataExportHandler : AbstractDataExportHandler
    {
        private readonly IBinaryDataSource binaryDataSource;

        protected AbstractExternalStorageDataExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IBinaryDataSource binaryDataSource) :
            base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;
        protected override bool CompressExportedData => false;
        
        protected override async Task ExportDataIntoDirectoryAsync(ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            using (this.GetClient(this.accessToken))
            {
                var applicationFolder = await this.CreateApplicationFolderAsync();

                string GetInterviewFolder(Guid interviewId) => $"{settings.QuestionnaireId}/{interviewId.FormatGuid()}";

                await binaryDataSource.ForEachInterviewMultimediaAsync(settings, 
                    async data =>
                    {
                        var interviewFolderPath = await this.CreateFolderAsync(applicationFolder, GetInterviewFolder(data.InterviewId));
                        await this.UploadFileAsync(interviewFolderPath, data.Content, data.FileName);

                    }, 
                    async audioAuditRecord =>
                    {
                        var interviewFolderPath = await this.CreateFolderAsync(applicationFolder, GetInterviewFolder(audioAuditRecord.InterviewId));
                        var audioFolder = await this.CreateFolderAsync(interviewFolderPath, interviewDataExportSettings.Value.AudioAuditFolderName);
                        await this.UploadFileAsync(audioFolder, audioAuditRecord.Content, audioAuditRecord.FileName);
                    }, 
                    progress, cancellationToken);
            }
        }

        public override Task ExportDataAsync(DataExportProcessArgs process, CancellationToken cancellationToken)
        {
            this.accessToken = process.AccessToken;
            return base.ExportDataAsync(process, cancellationToken);
        }

        protected abstract IDisposable GetClient(string accessToken);
        private string accessToken;

        protected abstract Task<string> CreateApplicationFolderAsync();
        protected abstract Task<string> CreateFolderAsync(string applicationFolder, string folderName);

        protected abstract Task UploadFileAsync(string folder, byte[] fileContent, string fileName);
    }
}
