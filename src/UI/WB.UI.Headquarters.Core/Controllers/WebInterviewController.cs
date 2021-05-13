#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
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
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Shared.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.UI.Headquarters.Code.WebInterview;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.UI.Headquarters.Controllers
{
    [AllowPrimaryWorkspaceFallback]
    [AllowAnonymousAttribute]
    [BrowsersRestriction]
    [TypeFilter(typeof(WebInterviewErrorFilterAttribute))]
    [Route("WebInterview")]
    public class WebInterviewController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IWebInterviewConfigProvider configProvider;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IUserViewFactory usersRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICaptchaProvider captchaProvider;
        private readonly IAssignmentsService assignments;
        private readonly IInvitationService invitationService;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummary;
        private readonly IInvitationMailingService invitationMailingService;
        private readonly IAggregateRootPrototypePromoterService promoterService;

        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage;
        private readonly IOptions<RecaptchaSettings> recaptchaSettings;
        private readonly IOptions<CaptchaConfig> captchaConfig;
        private readonly IServiceLocator serviceLocator;
        private readonly IAggregateRootPrototypeService prototypeService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;

        private const string CapchaCompletedKey = "CaptchaCompletedKey";
        private const string PasswordVerifiedKey = "PasswordVerifiedKey";
        public static readonly string LastCreatedInterviewIdKey = "lastCreatedInterviewId";
        public static readonly string AskForEmail = "askForEmail";

        private readonly ICalendarEventService calendarEventService;
        
        private readonly IMemoryCache memoryCache;
        private IWebInterviewLinkProvider webInterviewLinkProvider;

        private bool CapchaVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = HttpContext.Session.Get<List<string>>(CapchaCompletedKey);
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
            IStatefulInterviewRepository statefulInterviewRepository,
            IUserViewFactory usersRepository,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICaptchaProvider captchaProvider,
            IAssignmentsService assignments,
            IInvitationService invitationService,
            INativeReadSideStorage<InterviewSummary> interviewSummary,
            IInvitationMailingService invitationMailingService,
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IPlainKeyValueStorage<WebInterviewSettings> webInterviewSettingsStorage,
            IOptions<RecaptchaSettings> recaptchaSettings,
            IOptions<CaptchaConfig> captchaConfig,
            IServiceLocator serviceLocator,
            IAggregateRootPrototypeService prototypeService, 
            IQuestionnaireStorage questionnaireStorage, 
            IAggregateRootPrototypePromoterService promoterService,
            IMemoryCache memoryCache,
            ICalendarEventService calendarEventService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IWebInterviewLinkProvider webInterviewLinkProvider)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.usersRepository = usersRepository;
            this.keyGenerator = keyGenerator;
            this.captchaProvider = captchaProvider;
            this.assignments = assignments;
            this.invitationService = invitationService;
            this.interviewSummary = interviewSummary;
            this.invitationMailingService = invitationMailingService;
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.webInterviewSettingsStorage = webInterviewSettingsStorage;
            this.recaptchaSettings = recaptchaSettings;
            this.captchaConfig = captchaConfig;
            this.serviceLocator = serviceLocator;
            this.prototypeService = prototypeService;
            this.questionnaireStorage = questionnaireStorage;
            this.promoterService = promoterService;
            this.memoryCache = memoryCache;
            this.calendarEventService = calendarEventService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.webInterviewLinkProvider = webInterviewLinkProvider;
        }

        [Route("Error")]
        public ActionResult Error()
        {
            var errorMessage = TempData["WebInterview.ErrorMessage"] as string;
            return this.View("Error", new WebInterviewError()
            {
                ErrorMessage = errorMessage
            });
        }


        [WebInterviewAuthorize]
        [Route("{id:Guid}/Section/{sectionId}")]
        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }

            var sectionIdentity = Identity.Parse(sectionId);
            var targetSectionIsEnabled = interview.IsEnabled(sectionIdentity);
            if (!targetSectionIsEnabled)
            {
                return this.RedirectToFirstSection(id, interview);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Section", id, sectionId);
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);
            if (questionnaire.IsCoverPage(sectionIdentity.Id) && !IsNeedShowCoverPage(interview, questionnaire))
            {
                var firstSectionUrl = GenerateUrl(nameof(Section), id, questionnaire.GetFirstSectionId().FormatGuid());
                return Redirect(firstSectionUrl);
            }

            return this.View("Index", GetInterviewModel(id, interview, webInterviewConfig));
        }

        private WebInterviewIndexPageModel GetInterviewModel(string interviewId, IStatefulInterview interview, WebInterviewConfig webInterviewConfig)
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
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity) 
                                ?? throw new ArgumentNullException("Questionnaire not found");

            var config = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity);

            foreach (var messageKey in webInterviewConfig.CustomMessages.Keys.ToList())
            {
                var oldMessage = webInterviewConfig.CustomMessages[messageKey];
                webInterviewConfig.CustomMessages[messageKey] = 
                    SubstituteQuestionnaireName(oldMessage, questionnaire.Title);
            }
            
            return new WebInterviewIndexPageModel
            {
                Id = interviewId,
                CoverPageId = questionnaire.IsCoverPageSupported ? questionnaire.CoverPageSectionId.FormatGuid() : "",
                AskForEmail = askForEmail,
                CustomMessages = webInterviewConfig.CustomMessages,
                MayBeSwitchedToWebMode = config.Started && config.AllowSwitchToCawiForInterviewer && (interview.Mode != InterviewMode.CAWI),
                WebInterviewUrl = RenderWebInterviewUri(interview.GetAssignmentId() ?? 0, interview.Id)
            };
        }

        private string RenderWebInterviewUri(int assignmentId, Guid interviewId)
        {
            return webInterviewLinkProvider.WebInterviewRequestLink(assignmentId.ToString(), interviewId.ToString());
        }

        public string GenerateUrl(string action, string interviewId, string? sectionId = null)
        {
            return Url.Action(action, new
            {
                id = interviewId,
                sectionId = sectionId
            });
        }

        [HttpGet]
        [Route("{invitationId}/Start")]
        [AntiForgeryFilter]
        public IActionResult Start(string invitationId)
        {
            var invitation = GetInvitation(invitationId);

            bool DoesInterviewExist(string? interviewId)
            {
                if (interviewId == null) return false;
                return this.statefulInterviewRepository.Get(interviewId) != null;
            }

            if (invitation?.Assignment == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!invitation.IsWithAssignmentResolvedByPassword() && invitation.InterviewId != null)
            {
                if (DoesInterviewExist(invitation.InterviewId))
                    return this.RedirectToAction("Resume", routeValues: new { id = invitation.InterviewId });
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

            if (assignment.Quantity == 1)
            {
                // personal link
                if (!webInterviewConfig.UseCaptcha && string.IsNullOrWhiteSpace(assignment.Password))
                {
                    if (invitation.InterviewId != null)
                    {
                        if (DoesInterviewExist(invitation.InterviewId))
                        {
                            var interviewProperties = invitation.Interview.GetInterviewProperties();
                            if (!interviewProperties.AcceptsCAWIAnswers)
                                throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded,
                                    Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);
                            
                            HttpContext.Session.SaveWebInterviewAccessForCurrentUser(invitation.InterviewId);
                            return this.Redirect(GenerateUrl("Cover", invitation.InterviewId));
                        }
                    }

                    var interviewId = this.CreateInterview(assignment);
                    invitationService.InterviewWasCreated(invitation.Id, interviewId);
                    HttpContext.Session.SaveWebInterviewAccessForCurrentUser(interviewId);

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
                if (interviewIdCookie != null && Guid.TryParse(interviewIdCookie, out var pendingInterviewId) &&
                    webInterviewConfig.SingleResponse)
                {
                    if (DoesInterviewExist(invitation.InterviewId))
                    {
                        HttpContext.Session.SaveWebInterviewAccessForCurrentUser(invitation.InterviewId);
                        return this.Redirect(GenerateUrl("Cover", pendingInterviewId.FormatGuid()));
                    }
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

                    if (DoesInterviewExist(invitation.InterviewId))
                    {
                        Response.Cookies.Append($"InterviewId-{assignment.Id}", interviewId, new CookieOptions
                        {
                            Expires = DateTime.Now.AddYears(1)
                        });

                        HttpContext.Session.SaveWebInterviewAccessForCurrentUser(interviewId);
                        return this.Redirect(GenerateUrl("Cover", interviewId));
                    }
                }
            }

            var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
            model.ServerUnderLoad = false;

            return this.View(model);
        }

        public class SendLinkModel
        {
            public string? InterviewId { get; set; }
            public string? Email { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> EmailLink([FromBody]SendLinkModel data)
        {
            var interviewId = data.InterviewId;

            if (interviewId != null && Guid.TryParse(interviewId, out var aggregateId))
            {
                promoterService.MaterializePrototypeIfRequired(aggregateId);
            }

            var assignmentId = interviewSummary.GetById(data.InterviewId)?.AssignmentId ?? 0;
            var assignment = assignments.GetAssignment(assignmentId);

            int invitationId = invitationService.CreateInvitationForPublicLink(assignment, data.InterviewId);

            try
            {
                await invitationMailingService.SendResumeAsync(invitationId, assignment, data.Email);
                if (Request.Cookies[AskForEmail] != null)
                {
                    Response.Cookies.Delete(AskForEmail);
                }

                return this.Json("ok");
            }
            catch (EmailServiceException e)
            {
                invitationService.InvitationWasNotSent(invitationId, assignmentId, data.Email, e.Message);
                return this.Json("fail");
            }
        }

        [HttpPost]
        [Route("{invitationId}/Start")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartPost(string invitationId, [FromForm] string password)
        {
            var invitation = GetInvitation(invitationId);

            password ??= string.Empty;
            if (invitation == null)
                return NotFound();
            
            var assignment = invitation.Assignment;

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            if (!string.IsNullOrWhiteSpace(assignment.Password))
            {
                bool isValidPassword = invitation.IsWithAssignmentResolvedByPassword()
                    ? invitationService.IsValidTokenAndPassword(invitationId, password)
                    : assignment.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase);

                if (!isValidPassword)
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    model.IsPasswordInvalid = true;
                    return this.View("Start", model);
                }
            }

            if (webInterviewConfig.UseCaptcha)
            {
                var isCaptchaValid = await this.captchaProvider.IsCaptchaValid(Request);
                if (!isCaptchaValid)
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    model.CaptchaErrors = new List<string>() { Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha };
                    return this.View("Start", model);
                }
            }

            if (invitation.IsWithAssignmentResolvedByPassword())
            {
                invitation = invitationService.GetInvitationByTokenAndPassword(invitationId, password);
                assignment = invitation.Assignment;
            }

            if (invitation.InterviewId != null)
            {
                RememberCapchaFilled(invitation.InterviewId);
                HttpContext.Session.SaveWebInterviewAccessForCurrentUser(invitation.InterviewId);
                return this.Redirect(GenerateUrl("Cover", invitation.InterviewId));
            }

            var requestInterviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
            string stringValues = Request.Form["resume"];

            if (stringValues != null && Guid.TryParse(requestInterviewIdCookie, out Guid pendingInterviewId))
            {
                //interview could be deleted
                //if no answers were given
                if (this.statefulInterviewRepository.Get(pendingInterviewId.FormatGuid()) != null)
                {
                    if (invitation.InterviewId != null)
                    {
                        RememberCapchaFilled(invitation.InterviewId);
                        HttpContext.Session.SaveWebInterviewAccessForCurrentUser(invitation.InterviewId);
                    }

                    return this.Redirect(GenerateUrl("Cover", pendingInterviewId.FormatGuid()));
                }
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

            HttpContext.Session.SaveWebInterviewAccessForCurrentUser(interviewId);
            var generateUrl = GenerateUrl("Cover", interviewId);
            return this.Redirect(generateUrl);
        }

        [HttpGet]
        [Route("Continue/{id}")]
        public IActionResult Continue(string id)
        {
            var invitation = GetInvitation(id);

            if (invitation == null)
                return NotFound();

            var interview = this.statefulInterviewRepository.Get(invitation.InterviewId);

            if (interview == null)
                return NotFound();

            return this.RedirectToAction("Resume", routeValues: new { id = invitation.InterviewId });
        }

        [WebInterviewAuthorize]
        [Route("{id}/Cover")]
        [AntiForgeryFilter]
        public IActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null)
            {
                 throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                                    Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }
            
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(nameof(Cover), id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);

            if (IsNeedShowCoverPage(interview, questionnaire))
            {
                if (questionnaire.IsCoverPageSupported)
                {
                    var url = GenerateUrl(nameof(Section), id, questionnaire.CoverPageSectionId.FormatGuid());
                    return Redirect(url);
                }

                return View("Index", GetInterviewModel(id, interview, webInterviewConfig));
            }
            
            var firstSectionUrl = GenerateUrl(nameof(Section), id, questionnaire.GetFirstSectionId().FormatGuid());
            return Redirect(firstSectionUrl);
        }

        private bool IsNeedShowCoverPage(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            return questionnaire.GetPrefilledEntities().Any()
                   || !string.IsNullOrEmpty(interview.SupervisorRejectComment)
                   || interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Any();
        }

        [Route("Finish/{id:Guid}")]
        public ActionResult Finish(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);            
            if (interview == null ) return NotFound();

            if ((interview.IsCompleted || interview.Mode == InterviewMode.CAWI) 
                && this.IsAuthorizedUser(interview.CurrentResponsibleId))
            {
                return RedirectToAction("Completed", "InterviewerHq");
            }

            if(!interview.IsCompleted) return NotFound();

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) &&
                this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Finish", id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            var finishWebInterview = this.GetFinishModel(interview, webInterviewConfig);

            return View(finishWebInterview);
        }

        [WebInterviewAuthorize]
        [AntiForgeryFilter]
        [Route("Resume/{id:Guid}")]
        public ActionResult Resume(string id, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null)
            {
                var invitation = GetInvitation(id);
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
                HttpContext.Session.SaveWebInterviewAccessForCurrentUser(id);
                var model = this.GetResumeModel(id);
                return this.View("Resume", model);
            }

            RememberCapchaFilled(id);
            HttpContext.Session.SaveWebInterviewAccessForCurrentUser(id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(GenerateUrl(@"Cover", id));
        }

        [Route("{id:Guid}/Complete")]
        public ActionResult Complete(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }

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
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            return View("Index", GetInterviewModel(id, interview, webInterviewConfig));
        }

        [HttpPost]
        [Route("Resume/{id:Guid}")]
        [WebInterviewAuthorize]
        [AntiForgeryFilter]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResumePost(string id, string password, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null)
            {
                var invitation = GetInvitation(id);
                if (invitation == null)
                    return NotFound();

                interview = this.statefulInterviewRepository.Get(invitation.InterviewId);
            }

            if (interview == null)
                return NotFound();

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            if (webInterviewConfig.UseCaptcha && !await this.captchaProvider.IsCaptchaValid(Request))
            {
                var model = this.GetResumeModel(id);
                model.CaptchaErrors = new List<string>() { Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha };
                return this.View("Resume", model);
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
                        model.IsPasswordInvalid = true;
                        return this.View("Resume", model);
                    }
                }
            }

            RememberCapchaFilled(id);
            HttpContext.Session.SaveWebInterviewAccessForCurrentUser(id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(GenerateUrl(@"Cover", id));
        }

        [Route("Link/{assignmentId:int}/{interviewId:Guid}")]
        public ActionResult Link(int assignmentId, Guid interviewId)
        {
            var assignment = this.assignments.GetAssignment(assignmentId);

            if(assignment == null)
            {
                return Error();
            }

            var interview = this.interviewSummary.GetById(interviewId.FormatGuid());

            if(interview == null)
            {
                var model = GetLinkToWebInterviewModel(assignment);
                return View(model);
            }

            if(interview.InterviewMode != InterviewMode.CAWI)
            {
                return Finish(interview.SummaryId);
            }

            return RedirectToAction(nameof(Resume), new
            {
                id = interview.SummaryId
            });
        }

        private LinkToWebInterview GetLinkToWebInterviewModel(Assignment assignment)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignment.QuestionnaireId, null);

            if (questionnaire == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);

            string GetText(WebInterviewUserMessages messageType)
            {
                return SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(messageType).ToString(), questionnaire.Title);
            }

            return new LinkToWebInterview
            {
                QuestionnaireTitle = model.QuestionnaireTitle,
                UseCaptcha = model.UseCaptcha,
                HostedCaptchaHtml = model.HostedCaptchaHtml,
                RecaptchaSiteKey = model.RecaptchaSiteKey,
                HasPassword = model.HasPassword,
                CaptchaErrors = model.CaptchaErrors,
                IsPasswordInvalid = model.IsPasswordInvalid,
                LinkInvitation = GetText(WebInterviewUserMessages.LinkInvitation),
                LinkWelcome = GetText(WebInterviewUserMessages.LinkWelcome),
                SubmitUrl = Url.Action("Resume", "WebInterview"),
            };
        }

        [Route("OutdatedBrowser")]
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
            
            if (interviewer == null)
                throw new InvalidOperationException($"User was not found{assignment.ResponsibleId}");

            if (interviewer.Roles.Any(x => x == UserRoles.Supervisor || x == UserRoles.Headquarter))
                throw new InvalidOperationException(@"Web interview is not allowed to be completed by this role");

            var interviewId = Guid.NewGuid();
            var interviewKey = this.keyGenerator.Get();
            this.prototypeService.MarkAsPrototype(interviewId, PrototypeType.Temporary);

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                assignment.ProtectedVariables,
                interviewer.Supervisor?.Id ?? interviewer.PublicKey,
                interviewer.IsInterviewer() ? interviewer.PublicKey : (Guid?)null,
                interviewKey,
                assignment.Id,
                assignment.AudioRecording,
                InterviewMode.CAWI);

            this.commandService.Execute(createInterviewCommand);
            
            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignment.Id);
            if (calendarEvent != null)
            {
                var createCalendarEvent = new CreateCalendarEventCommand(Guid.NewGuid(), 
                    interviewer.PublicKey,
                    calendarEvent.Start.ToDateTimeUtc(),
                    calendarEvent.Start.Zone.Id,
                    interviewId,
                    interviewKey.ToString(),
                    assignment.Id,
                    calendarEvent.Comment);
                commandService.Execute(createCalendarEvent);
            }
            
            return interviewId.FormatGuid();
        }

        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound,
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);

            if (questionnaire == null)
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
                StartedDate = interview.StartedDate?.UtcDateTime.ToString("o"),
                QuestionnaireTitle = model.QuestionnaireTitle,
                UseCaptcha = model.UseCaptcha,
                HostedCaptchaHtml = model.HostedCaptchaHtml,
                RecaptchaSiteKey = model.RecaptchaSiteKey,
                HasPassword = model.HasPassword,
                CaptchaErrors = model.CaptchaErrors,
                IsPasswordInvalid = model.IsPasswordInvalid,
                ResumeWelcome = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.ResumeWelcome).ToString(),
                    questionnaire.Title),
                ResumeInvitation = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.ResumeInvitation).ToString(),
                    questionnaire.Title),
                ResumeButton = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.ResumeButton).ToString(),
                    questionnaire.Title),
                SubmitUrl = Url.Action("Resume", "WebInterview"),
            };
        }

        private StartWebInterview GetStartModel(
            QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig,
            Assignment? assignment)
        {
            var questionnaireBrowseItem = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            if (questionnaireBrowseItem == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var view = new StartWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                HasPassword = !string.IsNullOrWhiteSpace(assignment?.Password ?? String.Empty),
                WelcomeText = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.WelcomeText).ToString(),
                    questionnaireBrowseItem.Title),
                StartNewButton = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.StartNewButton).ToString(),
                    questionnaireBrowseItem.Title),
                ResumeButton = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.ResumeButton).ToString(),
                    questionnaireBrowseItem.Title),
                Description = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.Invitation).ToString(),
                    questionnaireBrowseItem.Title),
                CaptchaErrors = ModelState.ContainsKey("InvalidCaptcha") && ViewData.ModelState["InvalidCaptcha"].Errors.Any()
                                ? ViewData.ModelState["InvalidCaptcha"].Errors.Select(e => e.ErrorMessage).ToList()
                                : new List<string>(),
                IsPasswordInvalid = ViewData.ModelState.ContainsKey("InvalidPassword") && ViewData.ModelState["InvalidPassword"].Errors.Any(),
                SubmitUrl = Url.Action("Start", "WebInterview"),
                UseCaptcha = webInterviewConfig.UseCaptcha,
                RecaptchaSiteKey = webInterviewConfig.UseCaptcha && captchaConfig.Value.CaptchaType == CaptchaProviderType.Recaptcha ? recaptchaSettings.Value.SiteKey : null,
                HostedCaptchaHtml = webInterviewConfig.UseCaptcha && captchaConfig.Value.CaptchaType == CaptchaProviderType.Hosted ? serviceLocator.GetInstance<IHostedCaptcha>().Render<string>(null).Value : null,
            };

            if (assignment == null)
            {
                return view;
            }

            var interviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
            if (!Guid.TryParse(interviewIdCookie, out Guid pendingInterviewId))
            {
                return view;
            }

            var interview = statefulInterviewRepository.Get(pendingInterviewId.FormatGuid());

            if (interview?.Status == InterviewStatus.InterviewerAssigned)
            {
                view.HasPendingInterviewId = pendingInterviewId != Guid.Empty;
            }

            return view;
        }

        public static string SubstituteQuestionnaireName(string? template, string questionnaireName)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;

            return template.Replace("%QUESTIONNAIRE%", questionnaireName).Replace("%SURVEYNAME%", questionnaireName);
        }

        private FinishWebInterview GetFinishModel(IStatefulInterview interview, WebInterviewConfig webInterviewConfig)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);

            if (questionnaire == null)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var hasAccess = HttpContext.Session.HasAccessToWebInterviewAfterComplete(interview);
            var pdfUrl = hasAccess
                ? Url.Action("Pdf", "InterviewsPublicApi", new{ id = interview.Id })
                : null;
            
            return new FinishWebInterview
            {
                QuestionnaireTitle = questionnaire.Title,
                StartedDate = interview.StartedDate?.UtcDateTime.ToString("o"),
                CompletedDate = interview.CompletedDate?.UtcDateTime.ToString("o"),
                WebSurveyHeader = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.WebSurveyHeader).ToString(),
                    questionnaire.Title),
                FinishInterview = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.FinishInterview).ToString(),
                    questionnaire.Title),
                SurveyName = SubstituteQuestionnaireName(
                    webInterviewConfig.CustomMessages.GetText(WebInterviewUserMessages.SurveyName).ToString(),
                    questionnaire.Title),
                PdfUrl = pdfUrl,
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
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

        private Invitation? GetInvitation(string invitationId)
        {
            var key = "invitation::" + invitationId;

            if (this.memoryCache.TryGetValue(key, out object result)) return (Invitation?) result;

            var invitation = this.invitationService.GetInvitationByToken(invitationId);

            // do not crash on non existing invitation id
            if (invitation == null) return null;

            // only cache public web mode invitations
            if (invitation.Assignment.WebMode == true && invitation.Assignment.Quantity == null)
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - Triggering lazy load
                invitation.Assignment.Answers.Any();
                this.memoryCache.Set(key, invitation, TimeSpan.FromSeconds(30));

                return invitation;
            }

            return invitation;
        }
    }
}
