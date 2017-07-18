using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Captcha;
using WebInterview = WB.UI.Headquarters.Resources.WebInterview;

namespace WB.UI.Headquarters.Controllers
{
    [WebInterviewFeatureEnabled]
    [BrowsersRestriction]
    public class WebInterviewController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IWebInterviewConfigProvider configProvider;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IUserViewFactory usersRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICaptchaProvider captchaProvider;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IConnectionLimiter connectionLimiter;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private const string CapchaCompletedKey = "CaptchaCompletedKey";

        private bool CapchaVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = this.Session[CapchaCompletedKey] as List<string>;
            return !(passedInterviews?.Contains(interviewId)).GetValueOrDefault();
        }

        private void RememberCapchaFilled(string interviewId)
        {
            var interviews = this.Session[CapchaCompletedKey] as List<string> ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }
            this.Session[CapchaCompletedKey] = interviews;
        }

        public WebInterviewController(ICommandService commandService,
            IWebInterviewConfigProvider configProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IImageProcessingService imageProcessingService,
            IConnectionLimiter connectionLimiter,
            IWebInterviewNotificationService webInterviewNotificationService,
            ILogger logger, IUserViewFactory usersRepository,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICaptchaProvider captchaProvider,
            IPlainStorageAccessor<Assignment> assignments)
            : base(commandService, logger)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.imageProcessingService = imageProcessingService;
            this.connectionLimiter = connectionLimiter;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.usersRepository = usersRepository;
            this.keyGenerator = keyGenerator;
            this.captchaProvider = captchaProvider;
            this.assignments = assignments;
        }

        private string CreateInterview(Assignment assignment)
        {
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");

            var interviewer = this.usersRepository.GetUser(new UserViewInputModel(assignment.ResponsibleId));
            if (!interviewer.IsInterviewer())
                throw new InvalidOperationException($"Assignment {assignment.Id} has responsible that is not an interviewer. Interview cannot be created");

            var interviewId = Guid.NewGuid();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey, 
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                DateTime.UtcNow,
                interviewer.Supervisor.Id,
                interviewer.PublicKey,
                this.keyGenerator.Get(), 
                assignment.Id);

            this.commandService.Execute(createInterviewCommand);
            return interviewId.FormatGuid();
        }
        
        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            return new ResumeWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).UseCaptcha,
                StartedDate = interview.StartedDate
            };
        }

        private StartWebInterview GetStartModel(QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            var view = new StartWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = webInterviewConfig.UseCaptcha,
            };

            return view;
        }

        private FinishWebInterview GetFinishModel(IStatefulInterview interview)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            return new FinishWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                StartedDate = interview.StartedDate,
                CompletedDate = interview.CompletedDate
            };
        }

        private RedirectResult RedirectToFirstSection(string id, IStatefulInterview interview)
        {
            var sectionId = interview.GetAllEnabledGroupsAndRosters().First().Identity.ToString();
            var uri = GenerateUrl(@"Section", id, sectionId);

            return Redirect(uri);
        }

        
        [HttpPost]
        public async Task<ActionResult> Audio(string interviewId, string questionId, HttpPostedFileBase file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(interviewId);

            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question?.AsAudio != null)
            {
                return this.Json("fail");
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    byte[] bytes = ms.ToArray();
                    
                    //this.plainInterviewFileStorage.StoreInterviewBinaryData(interview.Id, filename, bytes);
                    //this.commandService.Execute(new AnswerAudioQuestionCommand(interview.Id,
                    //    responsibleId, questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, filename));
                }
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId, e.Message);
                throw;
            }
            return this.Json("ok");
        }

        [HttpPost]
        public async Task<ActionResult> Image(string interviewId, string questionId, HttpPostedFileBase file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(interviewId);

            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);
            
            if (!interview.AcceptsInterviewerAnswers() && question?.AsMultimedia != null)
            {
                return this.Json("fail");
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    this.imageProcessingService.ValidateImage(ms.ToArray());

                    var filename = $@"{question.VariableName}{string.Join(@"-", questionIdentity.RosterVector.Select(rv => rv))}{DateTime.UtcNow.GetHashCode()}.jpg";
                    var responsibleId = interview.CurrentResponsibleId;

                    this.plainInterviewFileStorage.StoreInterviewBinaryData(interview.Id, filename, ms.ToArray());
                    this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                        responsibleId, questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, filename));
                }
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId, e.Message);
                throw;
            }
            return this.Json("ok");
        }

        [WebInterviewAuthorize]
        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            var targetSectionIsEnabled = interview.IsEnabled(Identity.Parse(sectionId));
            if (!targetSectionIsEnabled)
            {
                return this.RedirectToFirstSection(id, interview);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Section", id, sectionId);
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            return this.View("Index");
        }

        public string GenerateUrl(string action, string interviewId, string sectionId = null)
        {
            return $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
        }

        public ActionResult Start(int id)
        {
            var assignment = this.assignments.GetById(id);
            if (assignment == null)
            {
                return this.HttpNotFound();
            }

            if (assignment.Archived || assignment.IsCompleted)
            {
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);

            var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig);
            model.ServerUnderLoad = !this.connectionLimiter.CanConnect();
            return this.View(model);
        }

        [HttpPost]
        [ActionName("Start")]
        [ValidateAntiForgeryToken]
        public ActionResult StartPost(int id)
        {
            var assignment = this.assignments.GetById(id);
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);

            if (!this.connectionLimiter.CanConnect())
            {
                var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig);
                model.ServerUnderLoad = true;
                return this.View(model);
            }

            if (webInterviewConfig.UseCaptcha)
            {
                if (!this.captchaProvider.IsCaptchaValid(this))
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig);
                    this.ModelState.AddModelError("InvalidCaptcha", WebInterview.PleaseFillCaptcha);
                    return this.View(model);
                }
            }

            var interviewId = this.CreateInterview(assignment);

            RememberCapchaFilled(interviewId);

            return this.Redirect(GenerateUrl("Cover", interviewId));
        }

        [WebInterviewAuthorize]
        public ActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(nameof(Cover), id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            return View("Index");
        }

        public ActionResult Finish(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null || !interview.IsCompleted) return this.HttpNotFound();

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Finish", id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            return View(this.GetFinishModel(interview));
        }

        [WebInterviewAuthorize]
        public ActionResult Resume(string id, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha)
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

        [WebInterviewAuthorize]
        public ActionResult Complete(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Complete", id);
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            return View("Index");
        }

        [HttpPost]
        [ActionName("Resume")]
        [WebInterviewAuthorize]
        public ActionResult ResumePost(string id, string returnUrl)
        {
            if (!this.captchaProvider.IsCaptchaValid(this))
            {
                var model = this.GetResumeModel(id);
                this.ModelState.AddModelError("InvalidCaptcha", WebInterview.PleaseFillCaptcha);
                return this.View(model);
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

        protected override void OnException(ExceptionContext filterContext)
        {
            var interviewException = filterContext.Exception.GetSelfOrInnerAs<InterviewException>();
            if (interviewException != null)
            {
                string errorMessage = "";
                switch (interviewException.ExceptionType)
                {
                    case InterviewDomainExceptionType.InterviewLimitReached:
                        errorMessage = WebInterview.ServerUnderLoad;
                        break;

                    case InterviewDomainExceptionType.QuestionnaireIsMissing:
                    case InterviewDomainExceptionType.InterviewHardDeleted:
                        errorMessage = WebInterview.Error_InterviewExpired;
                        break;
                    case InterviewDomainExceptionType.OtherUserIsResponsible:
                    case InterviewDomainExceptionType.StatusIsNotOneOfExpected:
                        errorMessage = WebInterview.Error_NoActionsNeeded;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                this.HandleInterviewAccessError(filterContext, errorMessage);
                return;
            }
            if (filterContext.Exception is HttpAntiForgeryException)
            {
                this.HandleInterviewAccessError(filterContext, WebInterview.Error_CookiesTurnedOff);
                return;
            }

            var interviewAccessException = filterContext.Exception as WebInterviewAccessException;
            if (interviewAccessException != null)
            {
                this.HandleInterviewAccessError(filterContext, interviewAccessException.Message);
                return;
            }

            this.HandleInDebugMode(filterContext);
        }


        [Conditional("DEBUG")]
        private void HandleInDebugMode(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            filterContext.Result = new ContentResult
            {
                Content = $@"
<h1>No <b>Index.cshtml<b> found in ~Views\WebInterview folder.</h1>
<p>Index.cshtml is generated by 'WB.UI.Headquarters.Interview' application build. </p>
<p>Please navigate to WB.UI.Headquarters.Interview folder and run following commands that will install all nodejs deps and run dev server</p>
<pre>
npm install 
npm run dev
</pre>
Exception details:<br />
<pre>{filterContext.Exception}</pre>"
            };
        }


        private void HandleInterviewAccessError(ExceptionContext filterContext, string message)
        {
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult
            {
                ViewName = @"~/Views/WebInterview/Error.cshtml",
                ViewData = new ViewDataDictionary(new WebInterviewError { Message = message })
            };
        }
    }
}