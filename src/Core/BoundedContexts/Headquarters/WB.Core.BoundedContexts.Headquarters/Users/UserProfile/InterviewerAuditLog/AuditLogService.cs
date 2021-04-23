using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog
{
    public class AuditLogService: IAuditLogService
    {
        private readonly IAuditLogFactory auditLogFactory;
        private readonly IAuthorizedUser authorizedUser;

        public AuditLogService(IAuditLogFactory auditLogFactory,
            IAuthorizedUser authorizedUser)
        {
            this.auditLogFactory = auditLogFactory;
            this.authorizedUser = authorizedUser;
        }

        public AuditLogQueryResult GetLastExisted7DaysRecords(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false)
        {
            if (!startDateTime.HasValue)
                startDateTime = DateTime.UtcNow.AddDays(1);

            var result = auditLogFactory.GetLastExisted7DaysRecords(id, startDateTime.Value);

            return new AuditLogQueryResult()
            {
                NextBatchRecordDate = result.NextBatchRecordDate,
                Items = GetAuditLogRecordItems(result.Records, showErrorMessage).ToArray()
            };
        }

        public IEnumerable<AuditLogRecordItem> GetAllRecords(Guid id, bool showErrorMessage = false)
        {
            var records = auditLogFactory.GetRecords(id);
            return GetAuditLogRecordItems(records, showErrorMessage);
        }

        public IEnumerable<AuditLogRecordItem> GetRecords(Guid id, DateTime start, DateTime end, bool showErrorMessage = false)
        {
            var records = auditLogFactory.GetRecords(id, start, end);
            return GetAuditLogRecordItems(records, showErrorMessage);
        }

        IEnumerable<AuditLogRecordItem> GetAuditLogRecordItems(IEnumerable<AuditLogRecord> records, bool showErrorMessage)
        {
            return records.Select(record => new AuditLogRecordItem()
            {
                Time = record.Time,
                Type = record.Type,
                Message = GetUserMessage(record, showErrorMessage),
                Description = GetMessageDescription(record, showErrorMessage),
            });
        }
        
        private string GetUserMessage(AuditLogRecord record, bool showErrorMessage)
        {
            switch (record.Type)
            {
                case AuditLogEntityType.CreateInterviewFromAssignment:
                    var createInterviewAuditLogEntity = record.GetEntity<CreateInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.CreateInterviewFromAssignment.FormatString(createInterviewAuditLogEntity.InterviewKey, createInterviewAuditLogEntity.AssignmentId);
                case AuditLogEntityType.OpenInterview:
                    var openInterviewAuditLogEntity = record.GetEntity<OpenInterviewAuditLogEntity>(); 
                    return InterviewerAuditRecord.OpenInterview.FormatString(openInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.CloseInterview:
                    var closeInterviewAuditLogEntity = record.GetEntity<CloseInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.CloseInterview.FormatString(closeInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.CompleteInterview:
                    var completeInterviewAuditLogEntity = record.GetEntity<CompleteInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.CompleteInterview.FormatString(completeInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.ApproveInterview:
                    var approveInterviewAuditLogEntity = record.GetEntity<ApproveInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.ApproveInterview.FormatString(approveInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.RejectInterview:
                    var rejectInterviewAuditLogEntity = record.GetEntity<RejectInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.RejectInterview.FormatString(rejectInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.DeleteInterview:
                    var deleteInterviewAuditLogEntity = record.GetEntity<DeleteInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.DeleteInterview.FormatString(deleteInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.Login:
                    var loginAuditLogEntity = record.GetEntity<LoginAuditLogEntity>();
                    return InterviewerAuditRecord.Login.FormatString(loginAuditLogEntity.UserName);
                case AuditLogEntityType.Logout:
                    var logoutAuditLogEntity = record.GetEntity<LogoutAuditLogEntity>();
                    return InterviewerAuditRecord.Logout.FormatString(logoutAuditLogEntity.UserName);
                case AuditLogEntityType.Relink:
                    //var relinkAuditLogEntity = record.GetEntity<RelinkAuditLogEntity>();
                    return InterviewerAuditRecord.Relink;
                case AuditLogEntityType.SynchronizationStarted:
                    var synchronizationStartedAuditLogEntity = record.GetEntity<SynchronizationStartedAuditLogEntity>();
                    return InterviewerAuditRecord.SynchronizationStarted.FormatString(synchronizationStartedAuditLogEntity.SynchronizationType);
                case AuditLogEntityType.SynchronizationCanceled:
                    return InterviewerAuditRecord.SynchronizationCanceled;
                case AuditLogEntityType.SynchronizationCompleted:
                    var synchronizationCompletedAuditLogEntity = record.GetEntity<SynchronizationCompletedAuditLogEntity>();
                    List<string> statusMessages = new List<string>();
                    if (synchronizationCompletedAuditLogEntity.NewAssignmentsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_AssignmentsDownloaded.FormatString(synchronizationCompletedAuditLogEntity.NewAssignmentsCount));
                    if (synchronizationCompletedAuditLogEntity.RemovedAssignmentsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_AssignmentsRemoved.FormatString(synchronizationCompletedAuditLogEntity.RemovedAssignmentsCount));
                    if (synchronizationCompletedAuditLogEntity.NewInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsDownloaded.FormatString(synchronizationCompletedAuditLogEntity.NewInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.SuccessfullyUploadedInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsUploaded.FormatString(synchronizationCompletedAuditLogEntity.SuccessfullyUploadedInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.RejectedInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsRejected.FormatString(synchronizationCompletedAuditLogEntity.RejectedInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.DeletedInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsRemoved.FormatString(synchronizationCompletedAuditLogEntity.DeletedInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.SuccessfullyPartialDownloadedInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsPartialDownloaded.FormatString(synchronizationCompletedAuditLogEntity.SuccessfullyPartialDownloadedInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.SuccessfullyPartialUploadedInterviewsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_InterviewsPartialUploaded.FormatString(synchronizationCompletedAuditLogEntity.SuccessfullyPartialUploadedInterviewsCount));
                    if (synchronizationCompletedAuditLogEntity.ReopenedInterviewsAfterReceivedCommentsCount > 0)
                        statusMessages.Add(InterviewerAuditRecord.SynchronizationCompleted_ReopenedInterviewsAfterComments.FormatString(synchronizationCompletedAuditLogEntity.ReopenedInterviewsAfterReceivedCommentsCount));

                    string statusMessage = statusMessages.Count == 0
                        ? InterviewerAuditRecord.SynchronizationCompleted_NothingToSync
                        : string.Join(", ", statusMessages);
                    return $"{InterviewerAuditRecord.SynchronizationCompleted} {statusMessage}";
                case AuditLogEntityType.SynchronizationFailed:
                    if (showErrorMessage && (authorizedUser?.IsAdministrator ?? false))
                    {
                        var synchronizationFailedAuditLogEntity = record.GetEntity<SynchronizationFailedAuditLogEntity>();
                        return InterviewerAuditRecord.SynchronizationFailed + @" : " + (synchronizationFailedAuditLogEntity.ExceptionMessage ?? @"<empty>");
                    }
                    return InterviewerAuditRecord.SynchronizationFailed;
                case AuditLogEntityType.OpenApplication:
                    //var openApplicationAuditLogEntity = record.GetEntity<OpenApplicationAuditLogEntity>();
                    return InterviewerAuditRecord.OpenApplication;
                case AuditLogEntityType.AssignResponsibleToInterview:
                    var assignResponsibleAuditLogEntity = record.GetEntity<AssignResponsibleToInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.AssignResponsibleToInterview.FormatString(assignResponsibleAuditLogEntity.ResponsibleName, assignResponsibleAuditLogEntity.InterviewKey);
                case AuditLogEntityType.AssignResponsibleToAssignment:
                    var assignResponsibleToAssignmentAuditLogEntity = record.GetEntity<AssignResponsibleToAssignmentAuditLogEntity>();
                    return InterviewerAuditRecord.AssignResponsibleToAssignment.FormatString(assignResponsibleToAssignmentAuditLogEntity.ResponsibleName, assignResponsibleToAssignmentAuditLogEntity.AssignmentId);
                case AuditLogEntityType.FinishInstallation:
                    return InterviewerAuditRecord.FinishInstallation.FormatString();
                case AuditLogEntityType.SwitchInterviewMode:
                    var switchInterviewModeAuditLogEntity = record.GetEntity<SwitchInterviewModeAuditLogEntity>();
                    return InterviewerAuditRecord.SwitchInterviewMode.FormatString(switchInterviewModeAuditLogEntity.InterviewKey, switchInterviewModeAuditLogEntity.Mode);
                case AuditLogEntityType.RestartInterview:
                    var restartInterviewLogEntity = record.GetEntity<RestartInterviewAuditLogEntity>();
                    return InterviewerAuditRecord.RestartInterview.FormatString(restartInterviewLogEntity.InterviewKey);
                default:
                    throw new ArgumentException("Unknown audit record type: " + record.Type);
            }
        }

        private string GetMessageDescription(AuditLogRecord record, bool showErrorMessage)
        {
            if (!showErrorMessage)
                return null;

            switch (record.Type)
            {
                case AuditLogEntityType.SynchronizationFailed:
                    if (authorizedUser?.IsAdministrator ?? false)
                    {
                        var synchronizationFailedAuditLogEntity = record.GetEntity<SynchronizationFailedAuditLogEntity>();
                        return synchronizationFailedAuditLogEntity.StackTrace;
                    }
                    return null;
                default:
                    return null;
            }
        }
    }
}
