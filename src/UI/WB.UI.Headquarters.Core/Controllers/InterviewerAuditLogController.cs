using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewerAuditLogController : Controller
    {
        private readonly IUserViewFactory usersRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IAuditLogService auditLogService;
        private readonly IAuthorizedUser authorizedUser;

        public InterviewerAuditLogController(
            IUserViewFactory usersRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IInterviewerVersionReader interviewerVersionReader,
            IAuditLogService auditLogService,
            IAuthorizedUser authorizedUser)
        {
            this.usersRepository = usersRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
            this.auditLogService = auditLogService;
            this.authorizedUser = authorizedUser;
        }

        public async Task<IActionResult> Index(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

            if (!startDateTime.HasValue)
                startDateTime = DateTime.UtcNow.AddDays(1);

            var result = auditLogService.GetLastExisted7DaysRecords(id, startDateTime.Value, showErrorMessage);

            var recordsByDate = new Dictionary<DateTime, List<AuditLogRecordItem>>();
            foreach (var record in result.Items)
            {
                if (!recordsByDate.ContainsKey(record.Time.Date))
                    recordsByDate.Add(record.Time.Date, new List<AuditLogRecordItem>());

                recordsByDate[record.Time.Date].Add(record);
            }

            var isNeedShowErrors = showErrorMessage && authorizedUser.IsAdministrator;

            var model = new InterviewerAuditLogModel();
            model.InterviewerName = userView.UserName;
            model.InterviewerId = userView.PublicKey;
            model.StartDateTime = result.NextBatchRecordDate;
            model.RecordsByDate = recordsByDate.Select(kv => new InterviewerAuditLogDateRecordsModel()
            {
                Date = kv.Key,
                RecordsByDate = kv.Value.Select(r => new InterviewerAuditLogRecordModel()
                {
                    Time = r.Time,
                    Type = r.Type,
                    Message = r.Message,
                    Description = isNeedShowErrors ? r.Description : null
                }).OrderByDescending(i => i.Time).ToArray()
            }).OrderByDescending(i => i.Date).ToArray();

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastByInterviewerId(id);
            if (lastSuccessDeviceInfo != null)
            {
                int? interviewerApkVersion = await interviewerVersionReader.InterviewerBuildNumber();
                var hasUpdateForInterviewerApp = interviewerApkVersion > lastSuccessDeviceInfo.AppBuildVersion;
                model.InterviewerAppVersion = lastSuccessDeviceInfo.AppVersion;
                model.DeviceId = lastSuccessDeviceInfo.DeviceId;
                model.DeviceModel = lastSuccessDeviceInfo.DeviceModel;
                model.HasUpdateForInterviewerApp = hasUpdateForInterviewerApp;
                model.HasDeviceInfo = true;
            }

            model.InterviewersUrl = Url.Action("Interviewers", "Users");
            model.ProfileUrl = Url.Action("Profile", "Interviewer", new {id = id});
            model.DownloadLogUrl = Url.Action("DownloadTabLog", new {id = id});
            model.SelfUrl = Url.Action("Index", new {id = id});
            return View(model);
        }

        public FileResult DownloadTabLog(Guid id, bool showErrorMessage = false)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

            var records = auditLogService.GetAllRecords(id);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null,
            };

            using MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);
            csvWriter.WriteField("Timestamp");
            csvWriter.WriteField("Action");
            csvWriter.NextRecord();

            foreach (var record in records)
            {
                csvWriter.WriteField(record.Time.ToString(CultureInfo.InvariantCulture));
                var message = record.Message;
                if (showErrorMessage && authorizedUser.IsAdministrator)
                {
                    message += Environment.NewLine + record.Description;
                }
                csvWriter.WriteField(message);
                csvWriter.NextRecord();
            }

            csvWriter.Flush();
            streamWriter.Flush();

            var fileContent = memoryStream.ToArray();
            return File(fileContent, "text/csv", $"actions_log_{userView.UserName}.tab");
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
        public string InterviewersUrl { get; set; }
        public string ProfileUrl { get; set; }
        
        public string DownloadLogUrl { get; set; }
        
        public string SelfUrl { get; set; }
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
        public string Description { get; set; }
    }
}
