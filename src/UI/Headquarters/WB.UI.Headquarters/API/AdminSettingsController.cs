using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class AdminSettingsController : ApiController
    {
        private readonly ISystemLogViewFactory systemLogViewFactory;

        public class GlobalNoticeModel
        {
            public string GlobalNotice { get; set; }
        }

        public class AutoUpdateModel
        {
            public bool InterviewerAutoUpdatesEnabled { get; set; }
            public int? HowManyMajorReleaseDontNeedUpdate { get; set; }
        }

        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;

        public AdminSettingsController(IPlainKeyValueStorage<GlobalNotice> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            ISystemLogViewFactory systemLogViewFactory)
        {
            this.appSettingsStorage = appSettingsStorage ?? throw new ArgumentNullException(nameof(appSettingsStorage));
            this.interviewerSettingsStorage = interviewerSettingsStorage ?? throw new ArgumentNullException(nameof(interviewerSettingsStorage));
            this.systemLogViewFactory = systemLogViewFactory ?? throw new ArgumentNullException(nameof(systemLogViewFactory));
        }

        [HttpGet]
        public HttpResponseMessage GlobalNoticeSettings()
        {
            return Request.CreateResponse(new GlobalNoticeModel
            {
                GlobalNotice = this.appSettingsStorage.GetById(AppSetting.GlobalNoticeKey)?.Message,
            });
        }

        [HttpPost]
        public HttpResponseMessage GlobalNoticeSettings([FromBody] GlobalNoticeModel message)
        {
            if (string.IsNullOrEmpty(message?.GlobalNotice))
            {
                this.appSettingsStorage.Remove(GlobalNotice.GlobalNoticeKey);
            }
            else
            {
                var globalNotice = this.appSettingsStorage.GetById(GlobalNotice.GlobalNoticeKey) ?? new GlobalNotice();
                globalNotice.Message = message.GlobalNotice.Length > 1000 ? message.GlobalNotice.Substring(0, 1000) : message.GlobalNotice;
                this.appSettingsStorage.Store(globalNotice, GlobalNotice.GlobalNoticeKey);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }

        [HttpGet]
        public HttpResponseMessage AutoUpdateSettings()
     {
            var interviewerSettings = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings);
            return Request.CreateResponse(new AutoUpdateModel
            {
                InterviewerAutoUpdatesEnabled = interviewerSettings.IsAutoUpdateEnabled()
            });
        }

        [HttpPost]
        public HttpResponseMessage AutoUpdateSettings([FromBody] AutoUpdateModel message)
        {
            this.interviewerSettingsStorage.Store(
                new InterviewerSettings
                {
                    AutoUpdateEnabled = message.InterviewerAutoUpdatesEnabled,
                },
                AppSetting.InterviewerSettings);

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }

        [HttpPost]
        public SystemLog GetSystemLog(SystemLogFilter filter) => this.systemLogViewFactory.GetLog(filter);

    }
}
