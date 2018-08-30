using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorSynchronizeHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;
        private readonly ISupervisorSettings settings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public SupervisorSynchronizeHandler(
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            ISupervisorSettings settings,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.interviewerViewRepository = interviewerViewRepository;
            this.settings = settings;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(CanSynchronize);
            requestHandler.RegisterHandler<GetLatestApplicationVersionRequest, GetLatestApplicationVersionResponse>(GetLatestApplicationVersion);
        }

        private Task<GetLatestApplicationVersionResponse> GetLatestApplicationVersion(
            GetLatestApplicationVersionRequest request)
            => Task.FromResult(new GetLatestApplicationVersionResponse
            {
                InterviewerApplicationVersion =
                    this.fileSystemAccessor.IsDirectoryExists(this.fileSystemAccessor.CombinePath(
                        this.settings.InterviewerAppPatchesDirectory, this.settings.GetApplicationVersionCode().ToString()))
                        ? this.settings.GetApplicationVersionCode()
                        : (int?)null
            });

        public Task<CanSynchronizeResponse> CanSynchronize(CanSynchronizeRequest arg)
        {
            if (settings.LastHqSyncTimestamp == null ||
                arg.LastHqSyncTimestamp != null && arg.LastHqSyncTimestamp > settings.LastHqSyncTimestamp)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.SupervisorRequireOnlineSync
                });
            }

            var supervisorBuildNumber = this.settings.GetApplicationVersionCode();
            if (supervisorBuildNumber < arg.InterviewerBuildNumber)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UnexpectedClientVersion
                });
            }

            var user = interviewerViewRepository.GetById(arg.InterviewerId.FormatGuid());
            if (user == null)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.NotATeamMember
                });
            }

            if (user.IsLockedByHeadquarters || user.IsLockedBySupervisor)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UserIsLocked
                });
            }

            return Task.FromResult(new CanSynchronizeResponse
            {
                CanSyncronize = true
            });
        }
    }
}
