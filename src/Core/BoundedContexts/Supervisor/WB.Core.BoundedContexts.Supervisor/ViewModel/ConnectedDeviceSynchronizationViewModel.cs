using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceSynchronizationViewModel : SynchronizationViewModelBase
    {
        public override bool IsSynchronizationInfoShowed => true;

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
                    return InterviewerUIResources.Synchronization_UploadingLogo;
                case SyncStage.UploadingAuditLog:
                    return InterviewerUIResources.Synchronization_ReceivingAuditLog;
                case SyncStage.AssignmentsSynchronization:
                    return syncProgressInfo.StageExtraInfo != null ?
                        InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount")) :
                        InterviewerUIResources.AssignmentsSynchronization;
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
                    return string.Format(InterviewerUIResources.Synchronization_Receiving_Title_Format,
                        InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text);
                case SyncStage.CheckNewVersionOfApplication:
                    return InterviewerUIResources.Synchronization_CheckNewVersionOfApplication;
                case SyncStage.DownloadApplication:
                    return InterviewerUIResources.Synchronization_UploadingApplication;
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
                case SyncStage.CheckObsoleteQuestionnaires:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires_Description,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount")) :
                        null;

                case SyncStage.AssignmentsSynchronization:
                case SyncStage.CheckForObsoleteInterviews:
                case SyncStage.DownloadingLogo:
                case SyncStage.UploadingAuditLog:
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
                case SyncStage.UploadInterviews:
                    return syncProgressInfo.StageExtraInfo != null
                        ? string.Format(InterviewerUIResources.Synchronization_Upload_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            InterviewerUIResources.Synchronization_Upload_Interviews_Text)
                        : null;
                case SyncStage.FailedUpgradeRequired:
                case SyncStage.CheckNewVersionOfApplication:
                    return null;
                case SyncStage.DownloadApplication:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(InterviewerUIResources.Synchronization_SendingApplication_Description,
                            syncProgressInfo.StageExtraInfo.GetOrNull("receivedKilobytes"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalKilobytes"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("receivingRate"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("progressPercentage")) :
                        null;
                case SyncStage.AttachmentsCleanup:
                    return null;
                case SyncStage.UpdatingAssignments:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            InterviewerUIResources.Synchronization_Interviews)
                        : InterviewerUIResources.TransferringAssignments;

                case SyncStage.UpdatingQuestionnaires:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            InterviewerUIResources.Synchronization_Questionnaires)
                        : InterviewerUIResources.UpdatingQuestionnaires;

                case SyncStage.Unknown:
                case null:

                default:
                    return syncProgressInfo.Description;
            }
        }
    }
}
