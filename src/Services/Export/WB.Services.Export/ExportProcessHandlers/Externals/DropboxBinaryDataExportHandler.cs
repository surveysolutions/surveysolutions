using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class DropboxBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        private DropboxClient client;

        public DropboxBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IBinaryDataSource binaryDataSource,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, binaryDataSource)
        {
        }

        protected override IDisposable GetClient(string accessToken)
        {
            this.client = new DropboxClient(accessToken);
            return client;
        }

        protected override Task<string> CreateApplicationFolderAsync() => Task.FromResult(string.Empty);

        protected override Task<string> CreateFolderAsync(string applicationFolder, string folderName) 
            => Task.FromResult($"/{folderName}");

        protected override async Task UploadFileAsync(string folder, byte[] fileContent, string fileName)
            => await this.client.Files.UploadAsync(new CommitInfo($"{folder}/{fileName}"), new MemoryStream(fileContent));
    }
}
