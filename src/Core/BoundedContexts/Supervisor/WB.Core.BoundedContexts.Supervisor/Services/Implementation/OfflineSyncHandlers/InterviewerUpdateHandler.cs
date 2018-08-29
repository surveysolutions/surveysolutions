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

            var patchFilePath = this.fileSystemAccessor.CombinePath(this.settings.InterviewerAppPatchesDirectory,
                this.settings.GetApplicationVersionCode().ToString(), patchFileName);

            return Task.FromResult(new GetInterviewerAppPatchResponse
            {
                Content = this.fileSystemAccessor.IsFileExists(patchFilePath) ? this.fileSystemAccessor.ReadAllBytes(patchFilePath) : null
            });
        }
    }
}
