using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Headquarters.Services;
using Filter = NLog.Filters.Filter;
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
        private readonly IStatefullWebInterviewFactory statefulInterviewRepository;
        private readonly IUserViewFactory usersRepository;
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
            IGlobalInfoProvider globalInfo,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IStatefullWebInterviewFactory statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IImageProcessingService imageProcessingService,
            IConnectionLimiter connectionLimiter,
            IWebInterviewNotificationService webInterviewNotificationService,
            ILogger logger, IUserViewFactory usersRepository)
            : base(commandService, globalInfo, logger)
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
        }

        private string CreateInterview(QuestionnaireIdentity questionnaireId)
        {
            var webInterviewConfig = this.configProvider.Get(questionnaireId);
            if (!webInterviewConfig.Started)
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");
            var responsibleId = webInterviewConfig.ResponsibleId;
            var interviewer = this.usersRepository.Load(new UserViewInputModel(responsibleId));

            var interviewId = Guid.NewGuid();
            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(interviewId,
                interviewer.PublicKey, questionnaireId, DateTime.UtcNow,
                interviewer.Supervisor.Id);

            this.commandService.Execute(createInterviewOnClientCommand);
            return interviewId.FormatGuid();
        }

        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            return new ResumeWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).UseCaptcha,
                InterviewId = this.statefulInterviewRepository.GetHumanInterviewId(interview.Id.FormatGuid()),
                StartedDate = interview.StartedDate
            };
        }

        private StartWebInterview GetStartModel(QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
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
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
            }

            return new FinishWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                InterviewId = this.statefulInterviewRepository.GetHumanInterviewId(interview.Id.FormatGuid()),
                StartedDate = interview.StartedDate,
                CompletedDate = interview.CompletedDate
            };
        }

        private RedirectToRouteResult RedirectToFirstSection(string id, IStatefulInterview interview)
        {
            return this.RedirectToAction("Section",
                new
                {
                    id,
                    sectionId = interview.GetAllEnabledGroupsAndRosters().First().Identity.ToString()
                });
        }

        [HttpPost]
        [WebInterviewAuthorize]
        public async Task<ActionResult> Image(string interviewId, string questionId, HttpPostedFileBase file)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);
            
            if (interview.Status != InterviewStatus.InterviewerAssigned && question?.AsMultimedia != null)
            {
                return this.Json("fail");
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    this.imageProcessingService.ValidateImage(ms.ToArray());

                    var filename = $@"{question.VariableName}{string.Join(@"-", questionIdentity.RosterVector.Select(rv => (int)rv))}{DateTime.UtcNow.GetHashCode().ToString()}.jpg";
                    var responsibleId = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).ResponsibleId;

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
                var returnUrl = Url.Action("Section", routeValues: new { id, sectionId });
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl = returnUrl });
            }

            return this.View("Index");
        }

        public ActionResult Start(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var webInterviewConfig = this.configProvider.Get(questionnaireIdentity);
            if (!webInterviewConfig.Started)
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);

            var model = this.GetStartModel(questionnaireIdentity, webInterviewConfig);
            model.ServerUnderLoad = !this.connectionLimiter.CanConnect();
            return this.View(model);
        }

        [HttpPost]
        [ActionName("Start")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartPost(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var webInterviewConfig = this.configProvider.Get(questionnaireIdentity);
            if (!webInterviewConfig.Started)
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);

            if (!this.connectionLimiter.CanConnect())
            {
                var model = this.GetStartModel(questionnaireIdentity, webInterviewConfig);
                model.ServerUnderLoad = true;
                return this.View(model);
            }

            if (webInterviewConfig.UseCaptcha)
            {
                var helper = this.GetRecaptchaVerificationHelper();
                var verifyResult = await helper.VerifyRecaptchaResponseTaskAsync();
                if (verifyResult != RecaptchaVerificationResult.Success)
                {
                    var model = this.GetStartModel(questionnaireIdentity, webInterviewConfig);
                    this.ModelState.AddModelError("InvalidCaptcha", WebInterview.PleaseFillCaptcha);
                    return this.View(model);
                }
            }

            var interviewId = this.CreateInterview(questionnaireIdentity);

            RememberCapchaFilled(interviewId);

            return this.RedirectToAction(nameof(Cover), new { id = interviewId });
        }

        [WebInterviewAuthorize]
        public ActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returlUrl = Url.Action(nameof(Cover), routeValues: new { id });
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returlUrl });
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
                var returlUrl = Url.Action("Finish", routeValues: new { id });
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returlUrl });
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
            return Redirect(returnUrl);
        }

        [WebInterviewAuthorize]
        public ActionResult Complete(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returlUrl = Url.Action("Complete", routeValues: new { id });
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returlUrl });
            }

            return View("Index");
        }

        [HttpPost]
        [ActionName("Resume")]
        [WebInterviewAuthorize]
        public async Task<ActionResult> ResumePost(string id, string returnUrl)
        {
            var verifyResult = await this.GetRecaptchaVerificationHelper().VerifyRecaptchaResponseTaskAsync();
            if (verifyResult != RecaptchaVerificationResult.Success)
            {
                var model = this.GetResumeModel(id);
                this.ModelState.AddModelError("InvalidCaptcha", WebInterview.PleaseFillCaptcha);
                return this.View(model);
            }

            RememberCapchaFilled(id);
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