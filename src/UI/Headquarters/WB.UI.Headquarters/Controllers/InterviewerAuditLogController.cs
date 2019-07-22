using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
    [LimitsFilter]
    public class InterviewerAuditLogController : BaseController
    {
        private readonly IUserViewFactory usersRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IAuditLogService auditLogService;

        public InterviewerAuditLogController(ICommandService commandService, 
            ILogger logger, 
            IUserViewFactory usersRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IInterviewerVersionReader interviewerVersionReader,
            IAuditLogService auditLogService) 
            : base(commandService, logger)
        {
            this.usersRepository = usersRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
            this.auditLogService = auditLogService;
        }

        public ActionResult Index(Guid id, DateTime? startDateTime = null, bool showErrorMessage = false)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

            if (!startDateTime.HasValue)
                startDateTime = DateTime.UtcNow.AddDays(1);

            var records = auditLogService.GetLastExisted7DaysRecords(id, startDateTime.Value, showErrorMessage);

            var model = new InterviewerAuditLogModel();
            model.InterviewerName = userView.UserName;
            model.InterviewerId = userView.PublicKey;
            model.StartDateTime = records.NextBatchRecordDate;
            model.RecordsByDate = records.Records.Select(kv => new InterviewerAuditLogDateRecordsModel()
            {
                Date = kv.Date,
                RecordsByDate = kv.RecordsByDate.Select(r => new InterviewerAuditLogRecordModel()
                {
                    Time = r.Time,
                    Type = r.Type,
                    Message = r.Message,
                    Description = r.Description
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

        public FileResult DownloadTabLog(Guid id, bool showErrorMessage = false)
        {
            var userView = usersRepository.GetUser(new UserViewInputModel(id));
            if (userView == null || (!userView.IsInterviewer() && !userView.IsSupervisor()))
                throw new InvalidOperationException($"User with id: {id} don't found");

            var fileContent = auditLogService.GenerateTabFile(id, showErrorMessage);
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
