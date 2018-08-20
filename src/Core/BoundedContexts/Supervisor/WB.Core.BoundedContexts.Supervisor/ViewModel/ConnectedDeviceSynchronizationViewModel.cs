using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceSynchronizationViewModel : SynchronizationViewModelBase
    {
        public override bool  IsSynchronizationInfoShowed => true;

        public override bool HasUserAnotherDevice => false;

        public override bool CanBeManaged => false;

        protected override void OnSyncCompleted()
        {
        }

        protected override void UpdateProcessStatus(SyncProgressInfo syncProgressInfo)
        {
            var title = GetLocalizedTitleOrDefault(syncProgressInfo);
            var description = GetLocalizedDescriptionOrDefault(syncProgressInfo);

            if (title != null || description != null)
            {
                this.ProcessOperation = title;
                this.ProcessOperationDescription = description;
                this.Status = syncProgressInfo.Status;
            }
        }

        private string GetLocalizedTitleOrDefault(SyncProgressInfo syncProgressInfo)
        {
            switch (syncProgressInfo.Stage)
            {
                case SyncStage.CheckForObsoleteInterviews:
                    return InterviewerUIResources.Synchronization_CheckForObsolete_Interviews;
                case SyncStage.CheckObsoleteQuestionnaires:
                    return InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires;
                case SyncStage.DownloadingLogo:
                    return InterviewerUIResources.Synchronization_DownloadingLogo;
                case SyncStage.UploadingAuditLog:
                    return InterviewerUIResources.Synchronization_UploadAuditLog;
                case SyncStage.AssignmentsSynchronization:
                    return InterviewerUIResources.AssignmentsSynchronization;
                case SyncStage.UserAuthentication:
                    return InterviewerUIResources.Synchronization_UserAuthentication_Title;
                case SyncStage.Success:
                    return InterviewerUIResources.Synchronization_Success_Title;
                case SyncStage.Stopped:
                    return null;
                case SyncStage.Canceled:
                case SyncStage.Failed:
                case SyncStage.FailedAccountIsLockedOnServer:
                    return InterviewerUIResources.Synchronization_Fail_Title;
                case SyncStage.FailedUserLinkedToAnotherDevice:
                    return InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Status;
                case SyncStage.FailedSupervisorShouldDoOnlineSync:
                    return InterviewerUIResources.Synchronization_SupervisorShouldDoOnlineSync_Title;
                case SyncStage.FailedUnacceptableSSLCertificate:
                case SyncStage.FailedUserDoNotBelongToTeam:
                    return InterviewerUIResources.UnexpectedException;
                case SyncStage.FailedUpgradeRequired:
                    return InterviewerUIResources.UpgradeRequired;
                case SyncStage.FailedUnexpectedException:
                    return InterviewerUIResources.Synchronization_Fail_Title;
                case SyncStage.UploadInterviews:
                    return string.Format(InterviewerUIResources.Synchronization_Upload_Title_Format,
                        InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text);
                case SyncStage.CheckNewVersionOfApplication:
                    return InterviewerUIResources.Synchronization_CheckNewVersionOfApplication;
                case SyncStage.DownloadApplication:
                    return InterviewerUIResources.Synchronization_DownloadApplication;
                case SyncStage.AttachmentsCleanup:
                    return InterviewerUIResources.Synchronization_Download_AttachmentsCleanup;
                case SyncStage.UpdatingAssignments:
                case SyncStage.UpdatingQuestionnaires:
                    return InterviewerUIResources.Synchronization_Download_Title;
                case SyncStage.Unknown:
                case null:
                default:
                    return syncProgressInfo.Title;
            }
        }

        private string GetLocalizedDescriptionOrDefault(SyncProgressInfo syncProgressInfo)
        {
            switch (syncProgressInfo.Stage)
            {
                case SyncStage.CheckForObsoleteInterviews:
                case SyncStage.CheckObsoleteQuestionnaires:
                case SyncStage.DownloadingLogo:
                case SyncStage.UploadingAuditLog:
                case SyncStage.AssignmentsSynchronization:
                    return null;
                case SyncStage.UserAuthentication:
                    return InterviewerUIResources.Synchronization_UserAuthentication_Description;
                case SyncStage.Success:
                    return InterviewerUIResources.Synchronization_Success_Description;
                case SyncStage.Stopped:
                    return null;
                case SyncStage.Canceled:
                case SyncStage.Failed:
                    return syncProgressInfo.Description;
                case SyncStage.FailedAccountIsLockedOnServer:
                    return InterviewerUIResources.AccountIsLockedOnServer;
                case SyncStage.FailedUserLinkedToAnotherDevice:
                    return InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Title;
                case SyncStage.FailedSupervisorShouldDoOnlineSync:
                    return InterviewerUIResources.Synchronization_SupervisorShouldDoOnlineSync;
                case SyncStage.FailedUnacceptableSSLCertificate:
                    return InterviewerUIResources.UnacceptableSSLCertificate;
                case SyncStage.FailedUserDoNotBelongToTeam:
                    return InterviewerUIResources.Synchronization_UserDoNotBelongToTeam;
                case SyncStage.FailedUnexpectedException:
                    return InterviewerUIResources.Synchronization_Fail_UnexpectedException;
                case SyncStage.FailedUpgradeRequired:
                case SyncStage.UploadInterviews:
                case SyncStage.CheckNewVersionOfApplication:
                case SyncStage.DownloadApplication:
                case SyncStage.AttachmentsCleanup:
                    return null;
                case SyncStage.UpdatingAssignments:
                    return InterviewerUIResources.TransferringAssignments;
                case SyncStage.UpdatingQuestionnaires:
                    return InterviewerUIResources.UpdatingQuestionnaires;

                case SyncStage.Unknown:
                case null:

                default:
                    return syncProgressInfo.Description;
            }
        }
    }
}
