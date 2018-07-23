using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
    [LimitsFilter]
    public class InterviewerAuditLogController : BaseController
    {
        private readonly IAuditLogFactory auditLogFactory;
        private readonly IUserViewFactory usersRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public InterviewerAuditLogController(ICommandService commandService, 
            ILogger logger, 
            IAuditLogFactory auditLogFactory,
            IUserViewFactory usersRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IInterviewerVersionReader interviewerVersionReader) 
            : base(commandService, logger)
        {
            this.auditLogFactory = auditLogFactory;
            this.usersRepository = usersRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        public ActionResult Index(Guid id, DateTime? startDateTime = null)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

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

            var model = new InterviewerAuditLogModel();
            model.InterviewerName = userView.UserName;
            model.InterviewerId = userView.PublicKey;
            model.StartDateTime = records.NextBatchRecordDate;
            model.RecordsByDate = recordsByDate.Select(kv => new InterviewerAuditLogDateRecordsModel()
            {
                Date = kv.Key,
                RecordsByDate = kv.Value.Select(r => new InterviewerAuditLogRecordModel()
                {
                    Time = r.Time,
                    Type = r.Type,
                    Message = GetUserMessage(r)
                }).OrderByDescending(i => i.Time).ToArray()
            }).OrderByDescending(i => i.Date).ToArray();

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastByInterviewerId(id);
            if (lastSuccessDeviceInfo != null)
            {
                int? interviewerApkVersion = interviewerVersionReader.Version;
                var hasUpdateForInterviewerApp = interviewerApkVersion > lastSuccessDeviceInfo.AppBuildVersion;
                model.InterviewerAppVersion = lastSuccessDeviceInfo.AppVersion;
                model.DeviceId = lastSuccessDeviceInfo.DeviceId;
                model.DeviceModel = lastSuccessDeviceInfo.DeviceModel;
                model.HasUpdateForInterviewerApp = hasUpdateForInterviewerApp;
                model.HasDeviceInfo = true;
            }

            return View(model);
        }

        public FileResult DownloadTabLog(Guid id)
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
                        csvWriter.WriteField(GetUserMessage(record));
                        csvWriter.NextRecord();
                    }

                    csvWriter.Flush();
                    streamWriter.Flush();

                    var fileContent = memoryStream.ToArray();
                    return File(fileContent, "text/csv", $"actions_log_{userView.UserName}.tab");
                }
            }
        }

        private string GetUserMessage(AuditLogRecord record)
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
                    //var synchronizationCanceledAuditLogEntity = record.GetEntity<SynchronizationCanceledAuditLogEntity>();
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
                    //var synchronizationCompletedAuditLogEntity = record.GetEntity<SynchronizationCompletedAuditLogEntity>();
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
    }

    public class InterviewerAuditLogModel
    {
        public string InterviewerName { get; set; }
        public Guid InterviewerId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public InterviewerAuditLogDateRecordsModel[] RecordsByDate { get; set; }
        public string InterviewerAppVersion { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public bool HasUpdateForInterviewerApp { get; set; }
        public bool HasDeviceInfo { get; set; }
    }

    public class InterviewerAuditLogDateRecordsModel
    {
        public DateTime Date { get; set; }
        public InterviewerAuditLogRecordModel[] RecordsByDate { get; set; }
    }

    public class InterviewerAuditLogRecordModel
    {
        public DateTime Time { get; set; }
        public AuditLogEntityType Type { get; set; }
        public string Message { get; set; }
    }
}
