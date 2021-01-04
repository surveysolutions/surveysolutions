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
                    return EnumeratorUIResources.Synchronization_CheckForObsolete_Interviews;
                case SyncStage.CheckObsoleteQuestionnaires:
                    return EnumeratorUIResources.Synchronization_Check_Obsolete_Questionnaires;
                case SyncStage.DownloadingLogo:
                    return EnumeratorUIResources.Synchronization_UploadingLogo;
                case SyncStage.UploadingAuditLog:
                    return EnumeratorUIResources.Synchronization_ReceivingAuditLog;
                case SyncStage.AssignmentsSynchronization:
                    return syncProgressInfo.StageExtraInfo != null ?
                        EnumeratorUIResources.Synchronization_Of_AssignmentsFormat.FormatString(
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount")) :
                        EnumeratorUIResources.AssignmentsSynchronization;
                case SyncStage.UserAuthentication:
                    return EnumeratorUIResources.Synchronization_UserAuthentication_Title;
                case SyncStage.Success:
                    return EnumeratorUIResources.Synchronization_Success_Title;
                case SyncStage.Stopped:
                    return null;
                case SyncStage.Canceled:
                case SyncStage.Failed:
                case SyncStage.FailedAccountIsLockedOnServer:
                    return EnumeratorUIResources.Synchronization_Fail_Title;
                case SyncStage.FailedUserLinkedToAnotherDevice:
                    return EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Status;
                case SyncStage.FailedSupervisorShouldDoOnlineSync:
                    return EnumeratorUIResources.Synchronization_SupervisorShouldDoOnlineSync_Title;
                case SyncStage.FailedUnacceptableSSLCertificate:
                case SyncStage.FailedUserDoNotBelongToTeam:
                    return EnumeratorUIResources.UnexpectedException;
                case SyncStage.FailedUpgradeRequired:
                    return EnumeratorUIResources.UpgradeRequired;
                case SyncStage.FailedUnexpectedException:
                    return EnumeratorUIResources.Synchronization_Fail_Title;
                case SyncStage.UploadInterviews:
                    return string.Format(EnumeratorUIResources.Synchronization_Receiving_Title_Format,
                        EnumeratorUIResources.Synchronization_Upload_CompletedAssignments_Text);
                case SyncStage.UploadingCalendarEvents:
                    return string.Format(EnumeratorUIResources.Synchronization_Receiving_Title_Format,
                        EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text);
                case SyncStage.CheckNewVersionOfApplication:
                    return EnumeratorUIResources.Synchronization_CheckNewVersionOfApplication;
                case SyncStage.DownloadApplication:
                    return EnumeratorUIResources.Synchronization_UploadingApplication;
                case SyncStage.AttachmentsCleanup:
                    return EnumeratorUIResources.Synchronization_Download_AttachmentsCleanup;
                case SyncStage.UpdatingAssignments:
                case SyncStage.UpdatingQuestionnaires:
                    return EnumeratorUIResources.Synchronization_Download_Title;
                case SyncStage.DownloadingCalendarEvents:
                    return EnumeratorUIResources.Synchronization_Download_CalendarEvents_Title;
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
                        string.Format(EnumeratorUIResources.Synchronization_Check_Obsolete_Questionnaires_Description,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount")) :
                        null;

                case SyncStage.AssignmentsSynchronization:
                case SyncStage.CheckForObsoleteInterviews:
                case SyncStage.DownloadingLogo:
                case SyncStage.UploadingAuditLog:
                    return null;
                case SyncStage.UserAuthentication:
                    return EnumeratorUIResources.Synchronization_UserAuthentication_Description;
                case SyncStage.Success:
                    return EnumeratorUIResources.Synchronization_Success_Description;
                case SyncStage.Stopped:
                    return null;
                case SyncStage.Canceled:
                case SyncStage.Failed:
                    return syncProgressInfo.Description;
                case SyncStage.FailedAccountIsLockedOnServer:
                    return EnumeratorUIResources.AccountIsLockedOnServer;
                case SyncStage.FailedUserLinkedToAnotherDevice:
                    return EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Title;
                case SyncStage.FailedSupervisorShouldDoOnlineSync:
                    return EnumeratorUIResources.Synchronization_SupervisorShouldDoOnlineSync;
                case SyncStage.FailedUnacceptableSSLCertificate:
                    return EnumeratorUIResources.UnacceptableSSLCertificate;
                case SyncStage.FailedUserDoNotBelongToTeam:
                    return EnumeratorUIResources.Synchronization_UserDoNotBelongToTeam;
                case SyncStage.FailedUnexpectedException:
                    return EnumeratorUIResources.Synchronization_Fail_UnexpectedException;
                case SyncStage.UploadInterviews:
                    return syncProgressInfo.StageExtraInfo != null
                        ? string.Format(EnumeratorUIResources.Synchronization_Upload_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            EnumeratorUIResources.Synchronization_Upload_Interviews_Text)
                        : null;
                case SyncStage.UploadingCalendarEvents:
                    return syncProgressInfo.StageExtraInfo != null
                        ? string.Format(EnumeratorUIResources.Synchronization_Upload_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text)
                        : null;
                case SyncStage.FailedUpgradeRequired:
                case SyncStage.CheckNewVersionOfApplication:
                    return null;
                case SyncStage.DownloadApplication:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(EnumeratorUIResources.Synchronization_SendingApplication_Description,
                            syncProgressInfo.StageExtraInfo.GetOrNull("receivedKilobytes"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalKilobytes"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("receivingRate"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("progressPercentage")) :
                        null;
                case SyncStage.AttachmentsCleanup:
                    return null;
                case SyncStage.UpdatingAssignments:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            EnumeratorUIResources.Synchronization_Interviews)
                        : EnumeratorUIResources.TransferringAssignments;
                case SyncStage.DownloadingCalendarEvents:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text)
                        : EnumeratorUIResources.TransferringCalendarEvents;
                case SyncStage.UpdatingQuestionnaires:
                    return syncProgressInfo.StageExtraInfo != null ?
                        string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            syncProgressInfo.StageExtraInfo.GetOrNull("processedCount"),
                            syncProgressInfo.StageExtraInfo.GetOrNull("totalCount"),
                            EnumeratorUIResources.Synchronization_Questionnaires)
                        : EnumeratorUIResources.UpdatingQuestionnaires;

                case SyncStage.Unknown:
                case null:

                default:
                    return syncProgressInfo.Description;
            }
        }
    }
}
