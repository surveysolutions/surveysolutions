using System;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class InterviewerUpdateHandler : IHandleCommunicationMessage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public InterviewerUpdateHandler(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetInterviewerAppPatchRequest, GetInterviewerAppPatchResponse>(GetInterviewerAppPatch);
        }

        private Task<GetInterviewerAppPatchResponse> GetInterviewerAppPatch(GetInterviewerAppPatchRequest request)
        {
            var patchFileName = $@"WBCapi.{request.AppVersion}{(request.AppType == EnumeratorApplicationType.WithMaps ? ".Ext" : "")}.delta";

            var pathToRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var patchesDirectory = this.fileSystemAccessor.CombinePath(pathToRootDirectory, "patches");
            var patchFilePath = this.fileSystemAccessor.CombinePath(patchesDirectory, patchFileName);

            return Task.FromResult(new GetInterviewerAppPatchResponse
            {
                Content = this.fileSystemAccessor.IsFileExists(patchFilePath) ? this.fileSystemAccessor.ReadAllBytes(patchFilePath) : null
            });
        }
    }
}
