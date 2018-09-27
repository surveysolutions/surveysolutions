using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
// using Microsoft.OneDrive.Sdk;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OnedriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        private  IGraphServiceClient graphServiceClient;
        
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

        protected override IDisposable GetClient(string accessToken)
        {
            graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));
            
            return null;
        }

        protected override Task<string> CreateApplicationFolderAsync() => Task.FromResult("Survey Solutions");

        protected override Task<string> CreateFolderAsync(string applicationFolder, string folderName)
            => Task.FromResult($"{applicationFolder}/{folderName}");

        protected override async Task UploadFileAsync(string folder, byte[] fileContent, string fileName)
        {
            var request = graphServiceClient.Drive.Root.ItemWithPath($"{folder}/{fileName}").Content.Request();
            await request.PutAsync<DriveItem>(new MemoryStream(fileContent));
        }
    }
}
