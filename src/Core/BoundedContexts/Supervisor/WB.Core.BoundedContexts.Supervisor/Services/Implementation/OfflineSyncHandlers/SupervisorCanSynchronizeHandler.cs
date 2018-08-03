using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorCanSynchronizeHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;
        private readonly IEnumeratorSettings settings;

        public SupervisorCanSynchronizeHandler(
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            IEnumeratorSettings settings)
        {
            this.interviewerViewRepository = interviewerViewRepository;
            this.settings = settings;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(CanSynchronize);
        }
        
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

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));

            if (expectedVersion.Revision != arg.InterviewerBuildNumber)
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

            //if (user.SecurityStamp != arg.SecurityStamp)
            //{
            //    return Task.FromResult(new CanSynchronizeResponse
            //    {
            //        CanSyncronize = false,
            //        Reason = SyncDeclineReason.InvalidPassword
            //    });
            //}

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
