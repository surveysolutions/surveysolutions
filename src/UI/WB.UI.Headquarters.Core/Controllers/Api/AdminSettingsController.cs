using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/{controller}/{action}")]
    public class AdminSettingsController : ControllerBase
    {
        private readonly ISystemLogViewFactory systemLogViewFactory;

        public class GlobalNoticeModel
        {
            public string GlobalNotice { get; set; }
        }

        public class InterviewerSettingsModel
        {
            public bool InterviewerAutoUpdatesEnabled { get; set; }
            public bool NotificationsEnabled { get; set; }
            public bool PartialSynchronizationEnabled { get; set; }
        }

        public class WebInterviewSettingsModel
        {
            public bool AllowEmails { get; set; }
        }

        public class ProfileSettingsModel
        {
            public bool AllowInterviewerUpdateProfile { get; set; }
        }

        public class TestEmailModel
        {
            public string Email{ get; set; }
        }

        private readonly IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage;
        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;
        private readonly IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage;

        private readonly IEmailService emailService;
        private readonly ISystemLog auditLog;
        private readonly IWebInterviewEmailRenderer emailRenderer;

        public AdminSettingsController(
            IPlainKeyValueStorage<GlobalNotice> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage,
            IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage,
            IEmailService emailService, 
            ISystemLog auditLog, 
            ISystemLogViewFactory systemLogViewFactory,
            IWebInterviewEmailRenderer emailRenderer)
        {
            this.appSettingsStorage = appSettingsStorage ?? throw new ArgumentNullException(nameof(appSettingsStorage));
            this.interviewerSettingsStorage = interviewerSettingsStorage ?? throw new ArgumentNullException(nameof(interviewerSettingsStorage));
            this.systemLogViewFactory = systemLogViewFactory ?? throw new ArgumentNullException(nameof(systemLogViewFactory));
            this.emailProviderSettingsStorage = emailProviderSettingsStorage ?? throw new ArgumentNullException(nameof(emailProviderSettingsStorage));
            this.profileSettingsStorage = profileSettingsStorage ?? throw new ArgumentNullException(nameof(profileSettingsStorage));
            
            this.webInterviewSettingsStorage = webInterviewSettingsStorage ??
                                               throw new ArgumentNullException(nameof(webInterviewSettingsStorage));
            this.emailService = emailService;
            this.auditLog = auditLog;
            this.emailRenderer = emailRenderer;
            
        }

        [HttpGet]
        public ActionResult<GlobalNoticeModel> GlobalNoticeSettings()
        {
            return new GlobalNoticeModel
            {
                GlobalNotice = this.appSettingsStorage.GetById(AppSetting.GlobalNoticeKey)?.Message,
            };
        }

        [HttpPost]
        public IActionResult GlobalNoticeSettings([FromBody] GlobalNoticeModel message)
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

            return Ok(new {sucess = true});
        }

        [HttpGet]
        public ActionResult<InterviewerSettingsModel> InterviewerSettings()
        {
            var interviewerSettings = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings);

            return new InterviewerSettingsModel
            {
                InterviewerAutoUpdatesEnabled = interviewerSettings.IsAutoUpdateEnabled(),
                NotificationsEnabled = interviewerSettings.IsDeviceNotificationsEnabled(),
                PartialSynchronizationEnabled = interviewerSettings.IsPartialSynchronizationEnabled(),
            };
        }

        [HttpPost]
        public IActionResult InterviewerSettings([FromBody] InterviewerSettingsModel message)
        {
            this.interviewerSettingsStorage.Store(
                new InterviewerSettings
                {
                    AutoUpdateEnabled = message.InterviewerAutoUpdatesEnabled,
                    DeviceNotificationsEnabled = message.NotificationsEnabled,
                    PartialSynchronizationEnabled = message.PartialSynchronizationEnabled,
                },
                AppSetting.InterviewerSettings);

            return Ok(new {sucess = true});
        }

        [HttpGet]
        public ActionResult<WebInterviewSettingsModel> WebInterviewSettings()
        {
            var webInterviewSettings = this.webInterviewSettingsStorage.GetById(AppSetting.WebInterviewSettings);

            return new WebInterviewSettingsModel
            {
                AllowEmails = webInterviewSettings?.AllowEmails ?? false
            };
        }

        [HttpPost]
        public IActionResult WebInterviewSettings([FromBody] WebInterviewSettingsModel message)
        {
            this.webInterviewSettingsStorage.Store(
                new WebInterviewSettings
                {
                    AllowEmails = message.AllowEmails
                },
                AppSetting.WebInterviewSettings);

            return Ok();
        }

        [HttpGet]
        public ActionResult<ProfileSettingsModel> ProfileSettings()
        {
            var profileSettings = this.profileSettingsStorage.GetById(AppSetting.ProfileSettings);

            return new ProfileSettingsModel
            {
                AllowInterviewerUpdateProfile = profileSettings?.AllowInterviewerUpdateProfile ?? false
            };
        }

        [HttpPost]
        public IActionResult ProfileSettings([FromBody] ProfileSettingsModel message)
        {
            this.profileSettingsStorage.Store(
                new ProfileSettings
                {
                    AllowInterviewerUpdateProfile = message.AllowInterviewerUpdateProfile,
                },
                AppSetting.ProfileSettings);

            return Ok(new {sucess = true});
        }

        [HttpPost]
        public IActionResult UpdateEmailProviderSettings([FromBody] EmailProviderSettings settings)
        {
            var currentsSettings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            this.emailProviderSettingsStorage.Store(settings, AppSetting.EmailProviderSettings);

            
            if (settings.Provider != (currentsSettings?.Provider ?? EmailProvider.None))
            {
                auditLog.EmailProviderWasChanged((currentsSettings?.Provider ?? EmailProvider.None).ToString(), settings.Provider.ToString());
            }

            return Ok(new {sucess = true});
        }

        [HttpGet]
        public ActionResult<EmailProviderSettings> EmailProviderSettings()
        {
            return this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailModel model)
        {
            var settings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            try
            {
                var template = new WebInterviewEmailTemplate(EmailTemplateTexts.InvitationTemplate.Subject, EmailTemplateTexts.InvitationTemplate.Message, EmailTemplateTexts.InvitationTemplate.PasswordDescription, EmailTemplateTexts.InvitationTemplate.LinkText);

                var email = await emailRenderer.RenderEmail(template, "XXXXXXXX", "#", 
                    "SURVEY NAME EXAMPLE", settings.Address, settings.SenderName);

                var sendingResult = await this.emailService.SendEmailAsync(model.Email, email.Subject, email.MessageHtml, email.MessageText);

                return Ok(new {success = true});
            }
            catch (EmailServiceException e)
            {
                return StatusCode((int)e.StatusCode, new
                {
                    Success = false,
                    Errors = e.Errors,
                    Email = e.Email
                });
            }
        }

        [HttpGet]
        public DataTableResponse<SystemLogItem> GetSystemLog(DataTableRequest request)
        {
            var systemLogFilter = new SystemLogFilter
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                SortOrder = request.GetSortOrderRequestItems()
            };
            var systemLog = this.systemLogViewFactory.GetLog(systemLogFilter);
            
            return new DataTableResponse<SystemLogItem>
            {
                Data = systemLog.Items,
                Draw = request.Draw + 1,
                RecordsFiltered = systemLog.TotalCount,
                RecordsTotal = systemLog.TotalCount
            };
        }
    }
}
