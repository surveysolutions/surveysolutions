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

        public Task<GetInterviewerAppResponse> GetInterviewerApp(GetInterviewerAppRequest request)
        {
            var patchFileName = $@"interviewer{
                (request.AppType == EnumeratorApplicationType.WithMaps ? ".maps" : "")
            }.apk";

            var patchFilePath = this.fileSystemAccessor.CombinePath(this.settings.InterviewerApplicationsDirectory,
                this.settings.GetApplicationVersionCode().ToString(), patchFileName);
            
            var result = new GetInterviewerAppResponse();
            
            if (!this.fileSystemAccessor.IsFileExists(patchFilePath))
            {
                result.Skipped = 0;
                result.Total = 0;
                return Task.FromResult(result);
            }

            result.Content = this.fileSystemAccessor.ReadAllBytes(patchFilePath, request.Skip, request.Maximum);
            result.Total = this.fileSystemAccessor.GetFileSize(patchFilePath);
            result.Skipped = request.Skip;
            result.Length = result.Content.Length;
            
            return Task.FromResult(result);
        }
    }
}
