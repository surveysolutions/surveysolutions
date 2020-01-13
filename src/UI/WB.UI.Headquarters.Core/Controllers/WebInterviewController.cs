using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Captcha;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [BrowsersRestriction]
    [WebInterviewErrorFilter]
    [Route("WebInterview/{id:Guid}")]
    public partial class WebInterviewController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IWebInterviewConfigProvider configProvider;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IUserViewFactory usersRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICaptchaProvider captchaProvider;
        private readonly IAssignmentsService assignments;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IConnectionLimiter connectionLimiter;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IPauseResumeQueue pauseResumeQueue;
        private readonly IInvitationService invitationService;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummary;
        private readonly IInvitationMailingService invitationMailingService;

        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage;

        private const string CapchaCompletedKey = "CaptchaCompletedKey";
        private const string PasswordVerifiedKey = "PasswordVerifiedKey";
        public static readonly string LastCreatedInterviewIdKey = "lastCreatedInterviewId";
        public static readonly string AskForEmail = "askForEmail";

        private bool CapchaVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = HttpContext.Session.Get<List<string>>(CapchaCompletedKey);
            return !(passedInterviews?.Contains(interviewId)).GetValueOrDefault();
        }

        private bool PasswordVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = HttpContext.Session.Get<List<string>>(PasswordVerifiedKey);
            return !(passedInterviews?.Contains(interviewId)).GetValueOrDefault();
        }

        private void RememberCapchaFilled(string interviewId)
        {
            var interviews = HttpContext.Session.Get<List<string>>(CapchaCompletedKey) ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }

            HttpContext.Session.Set(CapchaCompletedKey, interviews);
        }

        private void RememberPasswordVerified(string interviewId)
        {
            var interviews = HttpContext.Session.Get<List<string>>(PasswordVerifiedKey) ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }

            HttpContext.Session.Set(PasswordVerifiedKey, interviews);
        }

        public WebInterviewController(ICommandService commandService,
            IWebInterviewConfigProvider configProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService,
            IConnectionLimiter connectionLimiter,
            IWebInterviewNotificationService webInterviewNotificationService,
            ILogger logger, IUserViewFactory usersRepository,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICaptchaProvider captchaProvider,
            IAssignmentsService assignments,
            IAudioFileStorage audioFileStorage,
            IAudioProcessingService audioProcessingService,
            IPauseResumeQueue pauseResumeQueue,
            IInvitationService invitationService,
            INativeReadSideStorage<InterviewSummary> interviewSummary,
            IInvitationMailingService invitationMailingService,
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
            this.connectionLimiter = connectionLimiter;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.usersRepository = usersRepository;
            this.keyGenerator = keyGenerator;
            this.captchaProvider = captchaProvider;
            this.assignments = assignments;
            this.audioFileStorage = audioFileStorage;
            this.audioProcessingService = audioProcessingService;
            this.pauseResumeQueue = pauseResumeQueue;
            this.invitationService = invitationService;
            this.interviewSummary = interviewSummary;
            this.invitationMailingService = invitationMailingService;
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.webInterviewSettingsStorage = webInterviewSettingsStorage;
        }

        [WebInterviewAuthorize]
        [Route("Section/{sectionId}")]
        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            var targetSectionIsEnabled = interview.IsEnabled(Identity.Parse(sectionId));
            if (!targetSectionIsEnabled)
            {
                return this.RedirectToFirstSection(id, interview);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Section", id, sectionId);
                return this.RedirectToAction("Resume", routeValues: new {id, returnUrl});
            }

            LogResume(interview);
            return this.View("Index", GetInterviewModel(id, webInterviewConfig));
        }

        private WebInterviewIndexPageModel GetInterviewModel(string interviewId, WebInterviewConfig webInterviewConfig)
        {
            var emailSettings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);

            bool isAskForEmailAvailable =
                emailSettings != null && emailSettings.Provider != EmailProvider.None;

            if (isAskForEmailAvailable)
            {
                isAskForEmailAvailable =
                    this.webInterviewSettingsStorage.GetById(AppSetting.WebInterviewSettings)?.AllowEmails ?? false;
            }

            var askForEmail = isAskForEmailAvailable ? Request.Cookies[AskForEmail] ?? "false" : "false";

            return new WebInterviewIndexPageModel
            {
                Id = interviewId,
                AskForEmail = askForEmail,
                CustomMessages = webInterviewConfig.CustomMessages
            };
        }

        public string GenerateUrl(string action, string interviewId, string sectionId = null)
        {
            return $@"~/WebInterview/{interviewId}/{action}" +
                   (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
        }

        [Route("Start")]
        public ActionResult Start(string id)
        {
            var invitation = this.invitationService.GetInvitationByToken(id);

            if (invitation.Assignment == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!invitation.IsWithAssignmentResolvedByPassword() && invitation.InterviewId != null)
            {
                return this.RedirectToAction("Resume", routeValues: new {id = invitation.InterviewId});
            }

            var assignment = invitation.Assignment;

            if (assignment.Archived)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            if (assignment.WebMode == false)
            {
                // Compatibility issue. Every time users will use link,   they will create a new interview. All links will be bounced back
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            Guid pendingInterviewId = Guid.Empty;
            if (assignment.Quantity == 1)
            {
                // personal link
                if (!webInterviewConfig.UseCaptcha && string.IsNullOrWhiteSpace(assignment.Password))
                {
                    if (invitation.InterviewId != null)
                    {
                        if (invitation.Interview.Status >= InterviewStatus.Completed)
                            throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded,
                                Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);
                        return this.Redirect(GenerateUrl("Cover", invitation.InterviewId));
                    }

                    var interviewId = this.CreateInterview(assignment);
                    invitationService.InterviewWasCreated(invitation.Id, interviewId);

                    return this.Redirect(GenerateUrl("Cover", interviewId));
                }
                else
                {
                    // page should be shown
                }
            }
            else
            {
                // public mode
                var interviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
                if (interviewIdCookie != null && Guid.TryParse(interviewIdCookie, out pendingInterviewId) &&
                    webInterviewConfig.SingleResponse)
                {
                    return this.Redirect(GenerateUrl("Cover", pendingInterviewId.FormatGuid()));
                }

                if (assignment.IsCompleted)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                        Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

                if (invitation.InterviewId == null)
                {
                    Response.Cookies.Append(AskForEmail, "true", new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(7)
                    });
                }

                if (!webInterviewConfig.UseCaptcha && string.IsNullOrWhiteSpace(assignment.Password) &&
                    webInterviewConfig.SingleResponse)
                {
                    var interviewId = this.CreateInterview(assignment);

                    Response.Cookies.Append($"InterviewId-{assignment.Id}", interviewId, new CookieOptions
                    {
                        Expires = DateTime.Now.AddYears(1)
                    });

                    return this.Redirect(GenerateUrl("Cover", interviewId));
                }
            }

            var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
            model.ServerUnderLoad = !this.connectionLimiter.CanConnect();

            return this.View(model);
        }

        [HttpPost]
        [ActionName("Start")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartPost(string id, string password)
        {
            var invitation = this.invitationService.GetInvitationByToken(id);

            var assignment = invitation.Assignment;

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            if (!this.connectionLimiter.CanConnect())
            {
                var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, null);
                model.ServerUnderLoad = true;
                return this.View(model);
            }

            if (!string.IsNullOrWhiteSpace(assignment.Password))
            {
                bool isValidPassword = invitation.IsWithAssignmentResolvedByPassword()
                    ? invitationService.IsValidTokenAndPassword(id, password)
                    : assignment.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase);

                if (!isValidPassword)
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    this.ModelState.AddModelError("InvalidPassword", "Wrong password");
                    return this.View(model);
                }
            }

            if (webInterviewConfig.UseCaptcha)
            {
                var isCaptchaValid = await this.captchaProvider.IsCaptchaValid(Request);
                if (!isCaptchaValid)
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    this.ModelState.AddModelError("InvalidCaptcha",
                        Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha);
                    return this.View(model);
                }
            }

            if (invitation.IsWithAssignmentResolvedByPassword())
            {
                invitation = invitationService.GetInvitationByTokenAndPassword(id, password);
                assignment = invitation.Assignment;
            }

            if (invitation.InterviewId != null)
            {
                RememberCapchaFilled(invitation.InterviewId);
                return this.Redirect(GenerateUrl("Cover", invitation.InterviewId));
            }

            var requestInterviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
            string stringValues = Request.Form["resume"];
            if (stringValues != null && Guid.TryParse(requestInterviewIdCookie, out Guid pendingInterviewId))
            {
                RememberCapchaFilled(invitation.InterviewId);
                return this.Redirect(GenerateUrl("Cover", pendingInterviewId.FormatGuid()));
            }

            if (assignment.IsCompleted)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            var interviewId = this.CreateInterview(assignment);

            RememberCapchaFilled(interviewId);

            if (assignment.InPrivateWebMode())
            {
                invitationService.InterviewWasCreated(invitation.Id, interviewId);
            }

            Response.Cookies.Append($"InterviewId-{assignment.Id}", interviewId, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1)
            });

            if (!string.IsNullOrWhiteSpace(assignment.Password))
            {
                RememberPasswordVerified(interviewId);
            }

            TempData[LastCreatedInterviewIdKey] = interviewId;

            return this.Redirect(GenerateUrl("Cover", interviewId));
        }

        public IActionResult Continue(string id)
        {
            var invitation = this.invitationService.GetInvitationByToken(id);
            if (invitation == null)
                return NotFound();

            var interview = this.statefulInterviewRepository.Get(invitation.InterviewId);

            if (interview == null)
                return NotFound();

            return this.RedirectToAction("Resume", routeValues: new {id = invitation.InterviewId});
        }

        [WebInterviewAuthorize]
        [Route("Cover")]
        public IActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(nameof(Cover), id);
                return this.RedirectToAction("Resume", routeValues: new {id = id, returnUrl = returnUrl});
            }

            LogResume(interview);

            return View("Index", GetInterviewModel(id, webInterviewConfig));
        }

        private void LogResume(IStatefulInterview statefulInterview)
        {
            var lastCreatedInterview = TempData[LastCreatedInterviewIdKey] as string;
            if (lastCreatedInterview != statefulInterview.Id.FormatGuid())
            {
                this.pauseResumeQueue.EnqueueResume(new ResumeInterviewCommand(statefulInterview.Id,
                    statefulInterview.CurrentResponsibleId));
            }
        }

        [Route("Finish")]
        public ActionResult Finish(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null || !interview.IsCompleted) return NotFound();

            if (this.IsAuthorizedUser(interview.CurrentResponsibleId))
            {
                return RedirectToAction("Completed", "InterviewerHq");
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Finish", id);
                return this.RedirectToAction("Resume", routeValues: new {id = id, returnUrl = returnUrl});
            }

            var finishWebInterview = this.GetFinishModel(interview, webInterviewConfig);
            finishWebInterview.CustomMessages = webInterviewConfig.CustomMessages;

            return View(finishWebInterview);
        }

        [WebInterviewAuthorize]
        public ActionResult Resume(string id, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null)
            {
                var invitation = this.invitationService.GetInvitationByToken(id);
                if (invitation == null)
                    return NotFound();

                interview = this.statefulInterviewRepository.Get(invitation.InterviewId);
            }

            if (interview == null)
                return NotFound();

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            var assignmentId = interview.GetAssignmentId();
            var assignment = assignmentId.HasValue ? this.assignments.GetAssignment(assignmentId.Value) : null;

            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) ||
                !string.IsNullOrEmpty(assignment?.Password))
            {
                var model = this.GetResumeModel(id);
                return this.View("Resume", model);
            }

            RememberCapchaFilled(id);

            if (returnUrl == null)
            {
                return Redirect(GenerateUrl(@"Cover", id));
            }

            return Redirect(returnUrl);
        }

        [Route("Complete")]
        public ActionResult Complete(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            var isAuthorizedUser = this.IsAuthorizedUser(interview.CurrentResponsibleId);


            if (isAuthorizedUser && interview.Status == InterviewStatus.Completed)
            {
                return RedirectToAction("Completed", "InterviewerHq");
            }

            if (!isAuthorizedUser)
            {
                if (!webInterviewConfig.Started)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                        Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

                if (interview.Status == InterviewStatus.Completed)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded,
                        Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);
            }

            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Complete", id);
                return this.RedirectToAction("Resume", routeValues: new {id, returnUrl});
            }

            return View("Index", GetInterviewModel(id, webInterviewConfig));
        }

        [HttpPost]
        [ActionName("Resume")]
        [WebInterviewAuthorize]
        public async Task<ActionResult> ResumePost(string id, string password, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null)
            {
                var invitation = this.invitationService.GetInvitationByToken(id);
                if (invitation == null)
                    return NotFound();

                interview = this.statefulInterviewRepository.Get(invitation.InterviewId);
            }

            if (interview == null)
                return NotFound();

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            var isCaptchaValid = await this.captchaProvider.IsCaptchaValid(Request);
            if (webInterviewConfig.UseCaptcha && !isCaptchaValid)
            {
                var model = this.GetResumeModel(id);
                this.ModelState.AddModelError("InvalidCaptcha",
                    Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha);
                return this.View(model);
            }

            var assignmentId = interview.GetAssignmentId();
            if (assignmentId.HasValue)
            {
                var assignment = this.assignments.GetAssignment(assignmentId.Value);
                if (!string.IsNullOrWhiteSpace(assignment?.Password))
                {
                    bool isValidPassword =
                        assignment.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase);

                    if (!isValidPassword)
                    {
                        var model = this.GetResumeModel(id);
                        this.ModelState.AddModelError("InvalidPassword", "Wrong password");
                        return this.View(model);
                    }
                }
            }

            RememberCapchaFilled(id);

            if (returnUrl == null)
            {
                return Redirect(GenerateUrl(@"Cover", id));
            }

            return this.Redirect(returnUrl);
        }

        public ActionResult OutdatedBrowser()
        {
            return View();
        }

        private string CreateInterview(Assignment assignment)
        {
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");

            var interviewer = this.usersRepository.GetUser(assignment.ResponsibleId);

            if (interviewer.Roles.Any(x => x == UserRoles.Supervisor || x == UserRoles.Headquarter))
                throw new InvalidOperationException(@"Web interview is not allowed to be completed by this role");

            var interviewId = Guid.NewGuid();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                assignment.ProtectedVariables,
                interviewer.Supervisor?.Id ?? interviewer.PublicKey,
                interviewer.IsInterviewer() ? interviewer.PublicKey : (Guid?) null,
                this.keyGenerator.Get(),
                assignment.Id,
                assignment.AudioRecording);

            this.commandService.Execute(createInterviewCommand);
            return interviewId.FormatGuid();
        }

        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            var assignmentId = interview.GetAssignmentId();
            var assignment = assignmentId.HasValue ? this.assignments.GetAssignment(assignmentId.Value) : null;

            var model = this.GetStartModel(interview.QuestionnaireIdentity, webInterviewConfig, assignment);

            return new ResumeWebInterview
            {
                StartedDate = interview.StartedDate,
                QuestionnaireTitle = model.QuestionnaireTitle,
                UseCaptcha = model.UseCaptcha,
                CustomMessages = model.CustomMessages,
                HasPassword = model.HasPassword
            };
        }

        private StartWebInterview GetStartModel(
            QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig,
            Assignment assignment)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var view = new StartWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = webInterviewConfig.UseCaptcha,
                CustomMessages = webInterviewConfig.CustomMessages,
                HasPassword = !string.IsNullOrWhiteSpace(assignment?.Password ?? String.Empty)
            };

            var interviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
            if (Guid.TryParse(interviewIdCookie, out Guid pendingInterviewId))
            {
                var interview = statefulInterviewRepository.Get(pendingInterviewId.FormatGuid());
                if (interview.Status == InterviewStatus.InterviewerAssigned)
                {
                    view.PendingInterviewId = pendingInterviewId;
                }
            }

            return view;
        }

        private FinishWebInterview GetFinishModel(IStatefulInterview interview, WebInterviewConfig webInterviewConfig)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            return new FinishWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                StartedDate = interview.StartedDate,
                CompletedDate = interview.CompletedDate,
                CustomMessages = webInterviewConfig.CustomMessages
            };
        }

        private RedirectResult RedirectToFirstSection(string id, IStatefulInterview interview)
        {
            var sectionId = interview.GetAllEnabledGroupsAndRosters().First().Identity.ToString();
            var uri = GenerateUrl(@"Section", id, sectionId);

            return Redirect(uri);
        }

        private bool IsAuthorizedUser(Guid responsibleId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var isInterviewer = this.User.IsInRole(UserRoles.Interviewer.ToString());

                if (isInterviewer)
                {
                    if (Guid.TryParse(this.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userid))
                    {
                        return responsibleId == userid;
                    }
                }
            }

            return false;
        }
    }
}
