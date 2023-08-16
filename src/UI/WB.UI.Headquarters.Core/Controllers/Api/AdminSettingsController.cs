﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;
using WebInterviewSettings = WB.Core.BoundedContexts.Headquarters.DataExport.Security.WebInterviewSettings;

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

        public class InterviewerGeographyQuestionAccuracyInMetersModel
        {
            [Required]
            [Range(1, 1000)]
            public int GeographyQuestionAccuracyInMeters { get; set; }
        }

        public class InterviewerGeographyQuestionPeriodInSecondsModel
        {
            [Required]
            [Range(5, 1000)]
            public int GeographyQuestionPeriodInSeconds { get; set; }
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
        private readonly IExportFactory exportFactory;
        private readonly ILogger<AdminSettingsController> logger;

        public AdminSettingsController(
            IPlainKeyValueStorage<GlobalNotice> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage,
            IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage,
            IEmailService emailService, 
            ISystemLog auditLog, 
            ISystemLogViewFactory systemLogViewFactory,
            IWebInterviewEmailRenderer emailRenderer,
            IExportFactory exportFactory,
            ILogger<AdminSettingsController> logger)
        {
            this.appSettingsStorage = appSettingsStorage ?? throw new ArgumentNullException(nameof(appSettingsStorage));
            this.interviewerSettingsStorage = interviewerSettingsStorage ?? throw new ArgumentNullException(nameof(interviewerSettingsStorage));
            this.systemLogViewFactory = systemLogViewFactory ?? throw new ArgumentNullException(nameof(systemLogViewFactory));
            this.emailProviderSettingsStorage = emailProviderSettingsStorage ?? throw new ArgumentNullException(nameof(emailProviderSettingsStorage));
            this.profileSettingsStorage = profileSettingsStorage ?? throw new ArgumentNullException(nameof(profileSettingsStorage));
            
            this.webInterviewSettingsStorage = webInterviewSettingsStorage ??
                                               throw new ArgumentNullException(nameof(webInterviewSettingsStorage));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            this.auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
            this.emailRenderer = emailRenderer ?? throw new ArgumentNullException(nameof(emailRenderer));
            this.exportFactory = exportFactory ?? throw new ArgumentNullException(nameof(exportFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        [ValidateAntiForgeryToken]
        public IActionResult GlobalNoticeSettings([FromBody] GlobalNoticeModel message)
        {
            if (!ModelState.IsValid) return this.BadRequest();
            
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
        public ActionResult<object> InterviewerSettings()
        {
            var interviewerSettings = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings);

            return new
            {
                InterviewerAutoUpdatesEnabled = interviewerSettings.IsAutoUpdateEnabled(),
                NotificationsEnabled = interviewerSettings.IsDeviceNotificationsEnabled(),
                PartialSynchronizationEnabled = interviewerSettings.IsPartialSynchronizationEnabled(),
                GeographyQuestionAccuracyInMeters = interviewerSettings.GetGeographyQuestionAccuracyInMeters(),
                GeographyQuestionPeriodInSeconds = interviewerSettings.GetGeographyQuestionPeriodInSeconds(),
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InterviewerSettings([FromBody] InterviewerSettingsModel message)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateInterviewerSettings(settings =>
            {
                settings.AutoUpdateEnabled = message.InterviewerAutoUpdatesEnabled;
                settings.DeviceNotificationsEnabled = message.NotificationsEnabled;
                settings.PartialSynchronizationEnabled = message.PartialSynchronizationEnabled;
            });

            return Ok(new {sucess = true});
        }

        private void UpdateInterviewerSettings(Action<InterviewerSettings> updateAction)
        {
            var interviewerSettings = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings == null)
                interviewerSettings = new InterviewerSettings();
            
            updateAction.Invoke(interviewerSettings);
            
            this.interviewerSettingsStorage.Store(interviewerSettings, AppSetting.InterviewerSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InterviewerGeographyQuestionAccuracyInMeters([FromBody] InterviewerGeographyQuestionAccuracyInMetersModel message)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateInterviewerSettings(settings =>
            {
                settings.GeographyQuestionAccuracyInMeters = message.GeographyQuestionAccuracyInMeters;
            });

            return Ok(new {sucess = true});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InterviewerGeographyQuestionPeriodInSeconds([FromBody] InterviewerGeographyQuestionPeriodInSecondsModel message)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateInterviewerSettings(settings =>
            {
                settings.GeographyQuestionPeriodInSeconds = message.GeographyQuestionPeriodInSeconds;
            });

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
        [ValidateAntiForgeryToken]
        public IActionResult WebInterviewSettings([FromBody] WebInterviewSettingsModel message)
        {
            if (!ModelState.IsValid) return this.BadRequest();
            
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
        [ValidateAntiForgeryToken]
        public IActionResult ProfileSettings([FromBody] ProfileSettingsModel message)
        {
            if (!ModelState.IsValid) return this.BadRequest();
            
            this.profileSettingsStorage.Store(
                new ProfileSettings
                {
                    AllowInterviewerUpdateProfile = message.AllowInterviewerUpdateProfile,
                },
                AppSetting.ProfileSettings);

            return Ok(new {sucess = true});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateEmailProviderSettings([FromBody] EmailProviderSettings settings)
        {
            if (RegionEndpoint.EnumerableAllRegions.All(r => r.SystemName != settings.AwsRegion))
                return Ok(new {sucess = false, error = Settings.EmailProvider_AwsRegion_Unknown });
            
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
        [ValidateAntiForgeryToken]
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
                logger.LogError(e, "Error when send test email to {Email}", model.Email);
                return StatusCode((int)e.StatusCode, new
                {
                    Success = false,
                    Errors = e.Errors,
                    Email = e.Email
                });
            }
        }

        [HttpGet]
        public IActionResult GetSystemLog(DataTableRequest request, [FromQuery] string exportType)
        {
            if (!string.IsNullOrEmpty(exportType))
                return GetAllSystemLogFile(exportType);

            var systemLogFilter = new SystemLogFilter
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                SortOrder = request.GetSortOrderRequestItems()
            };
            var systemLog = this.systemLogViewFactory.GetLog(systemLogFilter);
            
            return new JsonResult(new DataTableResponse<SystemLogItem>
            {
                Data = systemLog.Items,
                Draw = request.Draw + 1,
                RecordsFiltered = systemLog.TotalCount,
                RecordsTotal = systemLog.TotalCount
            });
        }
        
        [HttpGet]
        public IActionResult GetAllSystemLogFile([FromQuery] string exportType)
        {
            Enum.TryParse(exportType, true, out ExportFileType type);

            var systemLogFilter = new SystemLogFilter
            {
                PageIndex = 1,
                PageSize = int.MaxValue,
                SortOrder = new[] { new OrderRequestItem() { Field = nameof(SystemLogItem.LogDate), Direction = OrderDirection.Desc },  }
            };
            var records = this.systemLogViewFactory.GetLog(systemLogFilter);
            var report = new ReportView()
            {
                Headers = new []{ "Log date", "User", "Event type", "Log"},
                Data = records.Items.Select(i => new object[] { i.LogDate, i.UserName, i.Type, i.Log }).ToArray(),
                TotalCount = records.TotalCount
            };

            var exportFile = this.exportFactory.CreateExportFile(type);
            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var fileNameStar = $@"audit_log{exportFile.FileExtension}";
            var result = File(exportFileStream, exportFile.MimeType, fileNameStar);
            return result;
        }
    }
}
