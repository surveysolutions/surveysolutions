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
            requestHandler.RegisterHandler<GetInterviewerAppRequest, GetInterviewerAppResponse>(GetInterviewerApp);
        }

        private Task<GetInterviewerAppResponse> GetInterviewerApp(GetInterviewerAppRequest request)
        {
            var patchFileName = $@"interviewer{(request.AppType == EnumeratorApplicationType.WithMaps ? ".maps" : "")}.apk";

            var patchFilePath = this.fileSystemAccessor.CombinePath(this.settings.InterviewerApplicationsDirectory,
                this.settings.GetApplicationVersionCode().ToString(), patchFileName);

            return Task.FromResult(new GetInterviewerAppResponse
            {
                Content = this.fileSystemAccessor.IsFileExists(patchFilePath) ? this.fileSystemAccessor.ReadAllBytes(patchFilePath) : null
            });
        }
    }
}
