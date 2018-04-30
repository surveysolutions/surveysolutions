using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
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
    [AuthorizeOr403(Roles = "Administrator")]
    [ObserverNotAllowed]
    [LimitsFilter]
    public class InterviewerAuditLogController : BaseController
    {
        private readonly IAuditLogFactory auditLogFactory;
        private readonly IUserViewFactory usersRepository;

        public InterviewerAuditLogController(ICommandService commandService, ILogger logger, 
            IAuditLogFactory auditLogFactory,
            IUserViewFactory usersRepository) : base(commandService, logger)
        {
            this.auditLogFactory = auditLogFactory;
            this.usersRepository = usersRepository;
        }

        public ActionResult Index(Guid id, DateTime? starDateTime, DateTime? endDateTime)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || !userView.IsInterviewer())
                throw new InvalidOperationException($"Interviewer with id: {id} not fpund");

            if (!starDateTime.HasValue)
                starDateTime = DateTime.UtcNow.Date.AddDays(-7);
            if (!endDateTime.HasValue)
                endDateTime = DateTime.UtcNow;

            var recordsFor7Days = auditLogFactory.GetRecords(id, starDateTime.Value, endDateTime.Value);

            var recordsByDate = new Dictionary<DateTime, List<AuditLogRecord>>();
            foreach (var record in recordsFor7Days)
            {
                if (!recordsByDate.ContainsKey(record.Time.Date))
                    recordsByDate.Add(record.Time.Date, new List<AuditLogRecord>());

                recordsByDate[record.Time.Date].Add(record);
            }

            var model = new InterviewerAuditLogModel();
            model.InterviewerName = userView.UserName;
            model.InterviewerId = userView.PublicKey;
            model.StartDateTime = starDateTime.Value;
            model.EndDateTime = endDateTime.Value;
            model.RecordsByDate = recordsByDate.Select(kv => new InterviewerAuditLogDateRecordsModel()
            {
                Date = kv.Key,
                RecordsByDate = kv.Value.Select(r => new InterviewerAuditLogRecordModel()
                {
                    Time = r.Time,
                    Type = r.Type,
                    Message = GetUserMessage(r)
                }).ToArray()
            }).ToArray();
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
                    var createInterviewAuditLogEntity = (CreateInterviewAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.CreateInterviewFromAssignment.FormatString(createInterviewAuditLogEntity.InterviewKey, createInterviewAuditLogEntity.AssignmentId);
                case AuditLogEntityType.OpenInterview:
                    var openInterviewAuditLogEntity = (OpenInterviewAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.OpenInterview.FormatString(openInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.CloseInterview:
                    var closeInterviewAuditLogEntity = (CloseInterviewAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.CloseInterview.FormatString(closeInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.CompleteInterview:
                    var completeInterviewAuditLogEntity = (CompleteInterviewAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.CompleteInterview.FormatString(completeInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.DeleteInterview:
                    var deleteInterviewAuditLogEntity = (DeleteInterviewAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.DeleteInterview.FormatString(deleteInterviewAuditLogEntity.InterviewKey);
                case AuditLogEntityType.Login:
                    var loginAuditLogEntity = (LoginAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.Login.FormatString(loginAuditLogEntity.UserName);
                case AuditLogEntityType.Logout:
                    var logoutAuditLogEntity = (LogoutAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.Logout.FormatString(logoutAuditLogEntity.UserName);
                case AuditLogEntityType.Relink:
                    //var relinkAuditLogEntity = (RelinkAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.Relink;
                case AuditLogEntityType.SynchronizationStarted:
                    //var synchronizationStartedAuditLogEntity = (SynchronizationStartedAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.SynchronizationStarted;
                case AuditLogEntityType.SynchronizationCanceled:
                    //var synchronizationCanceledAuditLogEntity = (SynchronizationCanceledAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.SynchronizationCanceled;
                case AuditLogEntityType.SynchronizationCompleted:
                    var synchronizationCompletedAuditLogEntity = (SynchronizationCompletedAuditLogEntity)record.Payload;
                    return InterviewerAuditRecord.SynchronizationCompleted;
                case AuditLogEntityType.OpenApplication:
                    //var openApplicationAuditLogEntity = (OpenApplicationAuditLogEntity)record.Payload;
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
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public InterviewerAuditLogDateRecordsModel[] RecordsByDate { get; set; }
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
