using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class AdminSettingsController : ApiController
    {
        public class SettingsModel
        {
            public string GlobalNotice { get; set; }
            public bool InterviewerAutoUpdatesEnabled { get; set; }
        }

        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;

        public AdminSettingsController(IPlainKeyValueStorage<GlobalNotice> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage)
        {
            if (appSettingsStorage == null) throw new ArgumentNullException(nameof(appSettingsStorage));
            this.appSettingsStorage = appSettingsStorage;
            this.interviewerSettingsStorage = interviewerSettingsStorage;
        }

        public HttpResponseMessage Get() => Request.CreateResponse(new SettingsModel
        {
            GlobalNotice = this.appSettingsStorage.GetById(AppSetting.GlobalNoticeKey)?.Message,
            InterviewerAutoUpdatesEnabled = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings)?.AutoUpdateEnabled ?? true
        });

        public HttpResponseMessage Post([FromBody] SettingsModel message)
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

            this.interviewerSettingsStorage.Store(
                new InterviewerSettings {AutoUpdateEnabled = message.InterviewerAutoUpdatesEnabled},
                AppSetting.InterviewerSettings);

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }
    }
}
