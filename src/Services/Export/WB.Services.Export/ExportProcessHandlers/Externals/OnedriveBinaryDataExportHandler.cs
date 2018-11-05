using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OnedriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        private readonly ILogger<OnedriveBinaryDataExportHandler> logger;
        private  IGraphServiceClient graphServiceClient;
        
        public OnedriveBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IBinaryDataSource binaryDataSource,
            IDataExportFileAccessor dataExportFileAccessor,
            ILogger<OnedriveBinaryDataExportHandler> logger)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, binaryDataSource)
        {
            this.logger = logger;
        }

        protected override IDisposable GetClient(string accessToken)
        {
            logger.LogInformation("Creating Microsoft.Graph.Client for OneDrive file upload");
            logger.LogTrace("Access_Token: " + accessToken);

            graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage => {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.CompletedTask;
            }));
            
            return null;
        }

        protected override Task<string> CreateApplicationFolderAsync() => Task.FromResult("Survey Solutions");

        protected override Task<string> CreateFolderAsync(string applicationFolder, string folderName)
            => Task.FromResult($"{applicationFolder}/{folderName}");

        protected override async Task UploadFileAsync(string folder, byte[] fileContent, string fileName)
        {
            if (fileContent.Length > 4 * 1024 * 1024)
            {
                logger.LogDebug($"Uploading {fileName} to {folder}. Large file of size {fileContent.Length} in chunks");
                const int maxSizeChunk = 320 * 4 * 1024;

                using (var ms = new MemoryStream(fileContent))
                {
                    var item = graphServiceClient.Drive.Root.ItemWithPath($"{folder}/{fileName}");
                    var session = await item.CreateUploadSession().Request().PostAsync();
                    var chunkUploader = new ChunkedUploadProvider(session, graphServiceClient, ms, maxSizeChunk);
                    await chunkUploader.UploadAsync();
                }
            }
            else
            {
                logger.LogDebug($"Uploading {fileName} to {folder}. Small file of size {fileContent.Length}");
                var item = graphServiceClient.Drive.Root.ItemWithPath($"{folder}/{fileName}");
                await item.Content.Request().PutAsync<DriveItem>(new MemoryStream(fileContent));
            }
        }
    }
}
