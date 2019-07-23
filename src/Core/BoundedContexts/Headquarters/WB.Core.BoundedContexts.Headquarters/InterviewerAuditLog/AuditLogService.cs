using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public class AuditLogService: IAuditLogService
    {
        private readonly IUserViewFactory usersRepository;
        private readonly IAuditLogFactory auditLogFactory;
        private readonly IAuthorizedUser authorizedUser;

        public AuditLogService(IUserViewFactory usersRepository,
            IAuditLogFactory auditLogFactory,
            IAuthorizedUser authorizedUser)
        {
            this.usersRepository = usersRepository;
            this.auditLogFactory = auditLogFactory;
            this.authorizedUser = authorizedUser;
        }

        public InterviewerAuditLogResult GetLastExisted7DaysRecords(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false)
        {
            if (!startDateTime.HasValue)
                startDateTime = DateTime.UtcNow.AddDays(1);

            var records = auditLogFactory.GetLastExisted7DaysRecords(id, startDateTime.Value);

            var recordsByDate = new Dictionary<DateTime, List<AuditLogRecord>>();
            foreach (var record in records.Records)
            {
                if (!recordsByDate.ContainsKey(record.Time.Date))
                    recordsByDate.Add(record.Time.Date, new List<AuditLogRecord>());

                recordsByDate[record.Time.Date].Add(record);
            }

            return new InterviewerAuditLogResult()
            {
                NextBatchRecordDate = records.NextBatchRecordDate,
                Records = recordsByDate.Select(kv => new InterviewerAuditLogDateRecords()
                {
                    Date = kv.Key,
                    RecordsByDate = kv.Value.Select(r => new InterviewerAuditLogRecord()
                    {
                        Time = r.Time,
                        Type = r.Type,
                        Message = GetUserMessage(r, showErrorMessage),
                        Description = GetMessageDescription(r, showErrorMessage)
                    }).OrderByDescending(i => i.Time).ToArray()
                }).OrderByDescending(i => i.Date).ToArray()
            }; 
        }

        public byte[] GenerateTabFile(Guid id, bool showErrorMessage = false)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

            var records = auditLogFactory.GetRecords(id);

            var csvConfiguration = new Configuration
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null,
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                {
                    csvWriter.WriteField("Timestamp");
                    csvWriter.WriteField("Action");
                    csvWriter.NextRecord();

                    foreach (var record in records)
                    {
                        csvWriter.WriteField(record.Time.ToString(CultureInfo.InvariantCulture));
                        var message = GetUserMessage(record, showErrorMessage);
                        if (showErrorMessage && authorizedUser.IsAdministrator)
                        {
                            message += "\r\n" + GetMessageDescription(record, showErrorMessage);
                        }
                        csvWriter.WriteField(message);
                        csvWriter.NextRecord();
                    }

                    csvWriter.Flush();
                    streamWriter.Flush();

                    var fileContent = memoryStream.ToArray();
                    return fileContent;
                }
            }
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
