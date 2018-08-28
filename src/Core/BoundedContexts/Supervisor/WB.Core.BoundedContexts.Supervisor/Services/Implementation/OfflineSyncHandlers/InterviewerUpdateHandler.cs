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
        private readonly ISupervisorSettings settings;

        public InterviewerUpdateHandler(IFileSystemAccessor fileSystemAccessor, ISupervisorSettings settings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
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
            var patchesDirectoryForSupervisorApp = this.fileSystemAccessor.CombinePath(patchesDirectory, this.settings.GetApplicationVersionCode().ToString());
            var patchFilePath = this.fileSystemAccessor.CombinePath(patchesDirectoryForSupervisorApp, patchFileName);

            return Task.FromResult(new GetInterviewerAppPatchResponse
            {
                Content = this.fileSystemAccessor.IsFileExists(patchFilePath) ? this.fileSystemAccessor.ReadAllBytes(patchFilePath) : null
            });
        }
    }
}
