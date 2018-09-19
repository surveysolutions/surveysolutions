using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OnedriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public OnedriveBinaryDataExportHandler(
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

        private OneDriveClient oneDriveClient;
        protected override IDisposable GetClient(string accessToken)
        {
            oneDriveClient = new OneDriveClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("bearer", accessToken);

                        return Task.CompletedTask;
                    }));

            return null;
        }

        protected override Task<string> CreateApplicationFolderAsync() => Task.FromResult("Survey Solutions");

        protected override Task<string> CreateFolderAsync(string applicationFolder, string folderName) 
            =>Task.FromResult($"{applicationFolder}/{folderName}");

        protected override async Task UploadFileAsync(string folder, byte[] fileContent, string fileName)
        {
            var request = oneDriveClient.Drive.Root.ItemWithPath($"{folder}/{fileName}").Content.Request();
            await request.PutAsync<Item>(new MemoryStream(fileContent));
        }
    }
}
