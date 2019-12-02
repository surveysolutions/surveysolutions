using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    [ObserverNotAllowed]
    [WebInterviewFeatureEnabled]
    public class WebInterviewSetupController : BaseController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAssignmentsService assignmentsService;
        private readonly IInvitationService invitationService;
        private readonly SendInvitationsTask sendInvitationsTask;
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IAuthorizedUser authorizedUser;

        // GET: WebInterviewSetup
        public WebInterviewSetupController(ICommandService commandService,
            ILogger logger, 
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IWebInterviewConfigurator configurator,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignments,
            IAssignmentsService assignmentsService,
            IWebInterviewNotificationService webInterviewNotificationService, 
            IInvitationService invitationService, 
            SendInvitationsTask sendInvitationsTask,
            IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            IAuthorizedUser authorizedUser)
            : base(commandService, 
                  logger)
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
                return this.HttpNotFound();
            }

            var model = new SendInvitationsModel
            {
                Api = new
                {
                    InvitationsInfo = Url.HttpRouteUrl("DefaultApiWithAction", new {controller = "WebInterviewSetupApi", action = "InvitationsInfo", id = id }),
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
            QuestionnaireIdentity questionnaireIdentity = QuestionnaireIdentity.Parse(Request["questionnaireId"]);

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return this.HttpNotFound();
            }
            
            if (this.invitationService.GetEmailDistributionStatus()?.Status != InvitationProcessStatus.InProgress)
            {
                this.invitationService.RequestEmailDistributionProcess(questionnaireIdentity, User.Identity.Name, questionnaire.Title);
            }

            await sendInvitationsTask.ScheduleRunAsync();
            return RedirectToAction("EmailDistributionProgress");
        }

        [ActivePage(MenuItem.Questionnaires)]
        public ActionResult EmailDistributionProgress()
        {
            var progress = this.invitationService.GetEmailDistributionStatus();
            if (progress == null)
            {
                return HttpNotFound();
            }

            return View(new EmailDistributionProgressModel { Api = new
            {
                StatusUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "WebInterviewSetupApi", action = "EmailDistributionStatus" }),
                CancelUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "WebInterviewSetupApi", action = "CancelEmailDistribution" }),
                ExportErrors = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "WebInterviewSetupApi", action = "ExportInvitationErrors" }),
                SurveySetupUrl = Url.Action("Index", "SurveySetup")
            }});
        }

        [ActivePage(MenuItem.Questionnaires)]
        public ActionResult Settings(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
            {
                return this.HttpNotFound();
            }

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return this.HttpNotFound();
            }

            var model = new SetupModel();
            model.QuestionnaireTitle = questionnaire.Title;
            model.QuestionnaireFullName = string.Format(Pages.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version);
            model.QuestionnaireIdentity = questionnaireIdentity;
            model.HasLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            model.LogoUrl = Url.Action("Thumbnail", "CompanyLogo", new { httproute = "DefaultApi" });
            model.SurveySetupUrl = Url.Action("Index", "SurveySetup");
            model.AssignmentsCount = this.assignmentsService.GetCountOfAssignmentsReadyForWebInterview(questionnaireIdentity);
            model.DownloadAssignmentsUrl = Url.HttpRouteUrl("DefaultApiWithAction",
                new { controller = "LinksExport", action = "Download", id = questionnaireIdentity.ToString() });

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

            return View(model);
        }

        private static string GetShortTitleForEmailTemplateGroup(EmailTextTemplateType type)
        {
            switch (type)
            {
                case EmailTextTemplateType.InvitationTemplate: return WebInterviewSettings.InvitationEmailMessage;
                case EmailTextTemplateType.ResumeTemplate: return WebInterviewSettings.ResumeEmailMessage;
                case EmailTextTemplateType.Reminder_NoResponse: return WebInterviewSettings.ReminderNoResponseEmailMessage;
                case EmailTextTemplateType.Reminder_PartialResponse: return WebInterviewSettings.ReminderPartialResponseEmailMessage;
                case EmailTextTemplateType.RejectEmail: return WebInterviewSettings.RejectEmailMessage;
                default:
                    throw new ArgumentException("Unknown email template type "+ type.ToString());
            }
        }

        private QuestionnaireBrowseItem FindQuestionnaire(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnarieId))
            {
                return null;
            }

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnarieId);
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
