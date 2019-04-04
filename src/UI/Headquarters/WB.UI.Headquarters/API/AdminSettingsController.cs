using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class AdminSettingsController : ApiController
    {
        public class GlobalNoticeModel
        {
            public string GlobalNotice { get; set; }
        }

        public class AutoUpdateModel
        {
            public bool InterviewerAutoUpdatesEnabled { get; set; }
            public int? HowManyMajorReleaseDontNeedUpdate { get; set; }
        }

        public class TestEmailModel
        {
            public string Email{ get; set; }
        }

        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;
        private readonly IEmailService emailService;
        private readonly IAuditLog auditLog;
        private readonly IWebInterviewEmailRenderer emailRenderer;

        public AdminSettingsController(
            IPlainKeyValueStorage<GlobalNotice> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage, IEmailService emailService, IAuditLog auditLog, 
            IWebInterviewEmailRenderer emailRenderer)
        {
            this.appSettingsStorage = appSettingsStorage ?? throw new ArgumentNullException(nameof(appSettingsStorage));
            this.interviewerSettingsStorage = interviewerSettingsStorage ?? throw new ArgumentNullException(nameof(interviewerSettingsStorage));
            this.emailProviderSettingsStorage = emailProviderSettingsStorage ?? throw new ArgumentNullException(nameof(emailProviderSettingsStorage));
            this.emailService = emailService;
            this.auditLog = auditLog;
            this.emailRenderer = emailRenderer;
            ;
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
        public HttpResponseMessage UpdateEmailProviderSettings([FromBody] EmailProviderSettings settings)
        {
            var currentsSettings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            this.emailProviderSettingsStorage.Store(settings, AppSetting.EmailProviderSettings);

            
            if (settings.Provider != (currentsSettings?.Provider ?? EmailProvider.None))
            {
                auditLog.EmailProviderWasChanged((currentsSettings?.Provider ?? EmailProvider.None).ToString(), settings.Provider.ToString());
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }

        [HttpGet]
        [CamelCase]
        public EmailProviderSettings EmailProviderSettings()
        {
            return this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
        }

        [HttpPost]
        [CamelCase]
        public async Task<HttpResponseMessage> SendTestEmail([FromBody] TestEmailModel model)
        {
            var settings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            try
            {
                var template = new WebInterviewEmailTemplate(EmailTemplateTexts.InvitationTemplate.Subject, EmailTemplateTexts.InvitationTemplate.Message, EmailTemplateTexts.InvitationTemplate.PasswordDescription, EmailTemplateTexts.InvitationTemplate.LinkText);

                var email = emailRenderer.RenderEmail(template, "XXXXXXXX", "#", 
                    "SURVEY NAME EXAMPLE", settings.Address, settings.SenderName);

                var sendingResult = await this.emailService.SendEmailAsync(model.Email, email.Subject, email.MessageHtml, email.MessageText);

                return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
            }
            catch (EmailServiceException e)
            {
                return Request.CreateResponse(e.StatusCode, new
                {
                    Success = false,
                    Errors = e.Errors,
                    Email = e.Email
                });
            }
        }
    }
}
