using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ObservingNotAllowed]
    public class WebInterviewSetupController : Controller
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAssignmentsService assignmentsService;
        private readonly IInvitationService invitationService;
        private readonly SendInvitationsTask sendInvitationsTask;
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IAuthorizedUser authorizedUser;

        // GET: WebInterviewSetup
        public WebInterviewSetupController(IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IAssignmentsService assignmentsService,
            IInvitationService invitationService, 
            SendInvitationsTask sendInvitationsTask,
            IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            IAuthorizedUser authorizedUser)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.assignmentsService = assignmentsService;
            this.invitationService = invitationService;
            this.sendInvitationsTask = sendInvitationsTask;
            this.appSettingsStorage = appSettingsStorage;
            this.authorizedUser = authorizedUser;
        }

        
        [ActivePage(MenuItem.Questionnaires)]
        public ActionResult SendInvitations(string id)
        {
            var status = this.invitationService.GetEmailDistributionStatus();

            if ((status?.Status ?? InvitationProcessStatus.Queued) == InvitationProcessStatus.InProgress)
            {
                return RedirectToAction(nameof(EmailDistributionProgress), new { questionnaireId = id });
            }

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return NotFound();
            }

            var model = new SendInvitationsModel
            {
                Api = new
                {
                    InvitationsInfo = Url.Action("InvitationsInfo", "WebInterviewSetupApi", new { id = id }),
                    SurveySetupUrl = Url.Action("Index", "SurveySetup"),
                    EmaiProvidersUrl = Url.Action("EmailProviders","Settings"),
                    WebSettingsUrl = Url.Action("Settings", "WebInterviewSetup", new { id = id }),
                },
                IsAdmin = authorizedUser.IsAdministrator
            };

            return View(model);
        }

        [ActivePage(MenuItem.Questionnaires)]
        [HttpPost]
        [ActionName("SendInvitations")]
        public async Task<ActionResult> SendInvitationsPost(string id)
        {
            QuestionnaireIdentity questionnaireIdentity = QuestionnaireIdentity.Parse(Request.Form["questionnaireId"]);

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return NotFound();
            }
            
            if (this.invitationService.GetEmailDistributionStatus()?.Status != InvitationProcessStatus.InProgress)
            {
                this.invitationService.RequestEmailDistributionProcess(questionnaireIdentity, this.authorizedUser.UserName, questionnaire.Title);
            }

            await sendInvitationsTask.ScheduleRunAsync();
            return RedirectToAction("EmailDistributionProgress");
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult EmailDistributionProgress()
        {
            var progress = this.invitationService.GetEmailDistributionStatus();
            if (progress == null)
            {
                return NotFound();
            }

            return View(new EmailDistributionProgressModel
            {
                Api = new
                {
                    StatusUrl = Url.Action("EmailDistributionStatus", "WebInterviewSetupApi"),
                    CancelUrl = Url.Action("CancelEmailDistribution", "WebInterviewSetupApi"),
                    ExportErrors = Url.Action("ExportInvitationErrors", "WebInterviewSetupApi"),
                    SurveySetupUrl = Url.Action("Index", "SurveySetup")
                }
            });
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult Settings(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
            {
                return NotFound();
            }

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return NotFound();
            }

            var model = new SetupModel();
            model.QuestionnaireTitle = questionnaire.Title;
            model.QuestionnaireVersion = questionnaire.Version;
            model.QuestionnaireIdentity = questionnaireIdentity;
            model.HasLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            model.LogoUrl = Url.Action("Thumbnail", "CompanyLogo", new { httproute = "DefaultApi" });
            model.SurveySetupUrl = Url.Action("Index", "SurveySetup");
            model.AssignmentsCount = this.assignmentsService.GetCountOfAssignmentsReadyForWebInterview(questionnaireIdentity);
            model.DownloadAssignmentsUrl = Url.Action("Download", "LinksExport", new {id = questionnaireIdentity.ToString() });

            model.TextOptions = Enum.GetValues(typeof(WebInterviewUserMessages)).Cast<WebInterviewUserMessages>()
                .ToDictionary(m => m.ToString().ToCamelCase(), m => m.ToUiString()).ToArray();
            model.DefaultTexts = WebInterviewConfig.DefaultMessages;
            model.TextDescriptions = Enum.GetValues(typeof(WebInterviewUserMessages)).Cast<WebInterviewUserMessages>()
                .ToDictionary(m => m, m => WebInterviewSetup.ResourceManager.GetString($"{nameof(WebInterviewUserMessages)}_{m}_Descr"));

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            model.DefinedTexts = config.CustomMessages;

            model.EmailTemplates = config.EmailTemplates.ToDictionary(t => t.Key, t =>
                new EmailTextTemplateViewModel()
                {
                    Subject = t.Value.Subject,
                    Message = t.Value.Message,
                    PasswordDescription = t.Value.PasswordDescription,
                    LinkText = t.Value.LinkText
                });
            model.DefaultEmailTemplates = WebInterviewConfig.DefaultEmailTemplates.ToDictionary(t => t.Key, t => 
                new EmailTextTemplateViewModel()
                {
                    ShortTitle = GetShortTitleForEmailTemplateGroup(t.Key),
                    Subject = t.Value.Subject,
                    Message = t.Value.Message,
                    PasswordDescription = t.Value.PasswordDescription,
                    LinkText = t.Value.LinkText
                });
            model.ReminderAfterDaysIfNoResponse = config.ReminderAfterDaysIfNoResponse;
            model.ReminderAfterDaysIfPartialResponse = config.ReminderAfterDaysIfPartialResponse;
            model.Started = config.Started;
            model.UseCaptcha = config.UseCaptcha;
            model.SingleResponse = config.SingleResponse;
            model.EmailOnComplete = config.EmailOnComplete;
            model.AttachAnswersInEmail = config.AttachAnswersInEmail;
            model.AllowSwitchToCawiForInterviewer = config.AllowSwitchToCawiForInterviewer;

            return View(model);
        }

        private static string GetShortTitleForEmailTemplateGroup(EmailTextTemplateType type)
        {
            return type switch
            {
                EmailTextTemplateType.InvitationTemplate => WebInterviewSettings.InvitationEmailMessage,
                EmailTextTemplateType.ResumeTemplate => WebInterviewSettings.ResumeEmailMessage,
                EmailTextTemplateType.Reminder_NoResponse => WebInterviewSettings.ReminderNoResponseEmailMessage,
                EmailTextTemplateType.Reminder_PartialResponse => WebInterviewSettings.ReminderPartialResponseEmailMessage,
                EmailTextTemplateType.RejectEmail => WebInterviewSettings.RejectEmailMessage,
                EmailTextTemplateType.CompleteInterviewEmail => WebInterviewSettings.CompleteEmailMessage,
                _ => throw new ArgumentException("Unknown email template type " + type.ToString()),
            };
        }

        private QuestionnaireBrowseItem FindQuestionnaire(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireId))
            {
                return null;
            }

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireId);
            return questionnaire;
        }
    }

    public class SendInvitationsModel
    {
        public dynamic Api { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class EmailDistributionProgressModel
    {
        public dynamic Api { get; set; }
    }

    public class SetupModel
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public int AssignmentsCount { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireFullName { get; set; }
        public string SurveySetupUrl { get; set; }
        public bool HasLogo { get; set; }
        public string LogoUrl { get; set; }
        public KeyValuePair<string, string>[] TextOptions { get; set; }
        public Dictionary<WebInterviewUserMessages, string> DefaultTexts { get; set; }
        public Dictionary<WebInterviewUserMessages, string> TextDescriptions { get; set; }
        public Dictionary<WebInterviewUserMessages, string> DefinedTexts { get; set; }
        public Dictionary<EmailTextTemplateType, EmailTextTemplateViewModel> EmailTemplates { get; set; }
        public Dictionary<EmailTextTemplateType, EmailTextTemplateViewModel> DefaultEmailTemplates { get; set; }
        public string DownloadAssignmentsUrl { get; set; }
        public int? ReminderAfterDaysIfNoResponse { get; set; } 
        public int? ReminderAfterDaysIfPartialResponse { get; set; }
        public bool Started { get; set; }
        public bool SingleResponse { get; set; }
        public long QuestionnaireVersion { get; set; }
        public bool EmailOnComplete { get; set; }
        public bool AttachAnswersInEmail { get; set; }
        public bool AllowSwitchToCawiForInterviewer { set;get;}
    }

    public class EmailTextTemplateViewModel
    {
        public string ShortTitle { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string PasswordDescription { get; set; }
        public string LinkText { get; set; }
    }
}
