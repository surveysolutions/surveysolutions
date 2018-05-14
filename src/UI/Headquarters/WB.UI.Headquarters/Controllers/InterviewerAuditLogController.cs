using System;
using System.Collections.Generic;
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
    [ObserverNotAllowed]
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

        public ActionResult Index(Guid id, DateTime? startDateTime)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || !userView.IsInterviewer())
                throw new InvalidOperationException($"Interviewer with id: {id} not fpund");

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
            int? interviewerApkVersion = interviewerVersionReader.Version;
            var hasUpdateForInterviewerApp = interviewerApkVersion.HasValue &&
                                         interviewerApkVersion.Value > lastSuccessDeviceInfo.AppBuildVersion;

            model.InterviewerAppVersion = lastSuccessDeviceInfo.AppVersion;
            model.DeviceId = lastSuccessDeviceInfo.DeviceId;
            model.DeviceModel = lastSuccessDeviceInfo.DeviceModel;
            model.HasUpdateForInterviewerApp = hasUpdateForInterviewerApp;

            return View(model);
        }

        public FileResult DownloadCsvLog(Guid id)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || !userView.IsInterviewer())
                throw new InvalidOperationException($"Interviewer with id: {id} not fpund");

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
                    csvWriter.WriteField("Time");
                    csvWriter.WriteField("Message");
                    csvWriter.NextRecord();

                    foreach (var record in records)
                    {
                        csvWriter.WriteField(record.Time);
                        csvWriter.WriteField(GetUserMessage(record));
                        csvWriter.NextRecord();
                    }

                    csvWriter.Flush();
                    streamWriter.Flush();

                    var fileContent = memoryStream.ToArray();
                    return File(fileContent, "text/csv", $"actions_log_{userView.UserName}.csv");
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
                    //var synchronizationStartedAuditLogEntity = record.GetEntity<SynchronizationStartedAuditLogEntity>();
                    return InterviewerAuditRecord.SynchronizationStarted;
                case AuditLogEntityType.SynchronizationCanceled:
                    //var synchronizationCanceledAuditLogEntity = record.GetEntity<SynchronizationCanceledAuditLogEntity>();
                    return InterviewerAuditRecord.SynchronizationCanceled;
                case AuditLogEntityType.SynchronizationCompleted:
                    var synchronizationCompletedAuditLogEntity = record.GetEntity<SynchronizationCompletedAuditLogEntity>();
                    return InterviewerAuditRecord.SynchronizationCompleted.FormatString(
                        synchronizationCompletedAuditLogEntity.NewAssignmentsCount,
                        synchronizationCompletedAuditLogEntity.RemovedAssignmentsCount,
                        synchronizationCompletedAuditLogEntity.NewInterviewsCount,
                        synchronizationCompletedAuditLogEntity.SuccessfullyUploadedInterviewsCount,
                        synchronizationCompletedAuditLogEntity.RejectedInterviewsCount,
                        synchronizationCompletedAuditLogEntity.DeletedInterviewsCount
                        );
                case AuditLogEntityType.SynchronizationFailed:
                    //var synchronizationCompletedAuditLogEntity = record.GetEntity<SynchronizationCompletedAuditLogEntity>();
                    return InterviewerAuditRecord.SynchronizationFailed;
                case AuditLogEntityType.OpenApplication:
                    //var openApplicationAuditLogEntity = record.GetEntity<OpenApplicationAuditLogEntity>();
                    return InterviewerAuditRecord.OpenApplication;
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
