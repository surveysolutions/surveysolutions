using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Headquarters.Resources;

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
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private const string CapchaCompletedKey = "CaptchaCompletedKey";

        public WebInterviewController(ICommandService commandService,
            IWebInterviewConfigProvider configProvider,
            IGlobalInfoProvider globalInfo,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            ILogger logger, IUserViewFactory usersRepository)
            : base(commandService, globalInfo, logger)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
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

        private void PreserveCaptchaFilledKey()
        {
            if (this.TempData.ContainsKey(CapchaCompletedKey))
            {
                this.TempData[CapchaCompletedKey] = this.TempData[CapchaCompletedKey];
            }
        }

        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            var model = new ResumeWebInterview();
            model.QuestionnaireTitle = questionnaireBrowseItem.Title;
            model.UseCaptcha = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).UseCaptcha;
            return model;
        }

        private StartWebInterview GetStartModel(QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            var model = new StartWebInterview();
            model.QuestionnaireTitle = questionnaireBrowseItem.Title;
            model.UseCaptcha = webInterviewConfig.UseCaptcha;
            return model;
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

        [HttpPost]
        public async Task<ActionResult> Image(string interviewId, string questionId, HttpPostedFileBase file)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);

            if (interview.Status != InterviewStatus.InterviewerAssigned && question?.AsMultimedia != null)
            {
                return this.Json("fail");
            }

            using (var ms = new MemoryStream())
            {
                await file.InputStream.CopyToAsync(ms);

                var filename = $@"{question.VariableName}{string.Join(@"-", questionIdentity.RosterVector.Select(rv => (int)rv))}{DateTime.UtcNow.GetHashCode().ToString()}.jpg";
                var responsibleId = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).ResponsibleId;

                this.plainInterviewFileStorage.StoreInterviewBinaryData(interview.Id, filename, ms.ToArray());
                this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                    responsibleId, questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, filename));
            }

            return this.Json("ok");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            this.HandleInDebugMode(filterContext);
        }

        [WebInterviewAuthorize]
        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            var targetSectionIsEnabled = interview.IsEnabled(Identity.Parse(sectionId));
            if (!targetSectionIsEnabled)
            {
                this.PreserveCaptchaFilledKey();
                return this.RedirectToFirstSection(id, interview);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && TempData[CapchaCompletedKey] == null)
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
                return this.HttpNotFound();

            var model = this.GetStartModel(questionnaireIdentity, webInterviewConfig);

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
                return this.HttpNotFound();

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
            TempData[CapchaCompletedKey] = true;
            return this.Redirect("~/WebInterview/" + interviewId + "/Cover");
        }

        [WebInterviewAuthorize]
        public ActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && TempData[CapchaCompletedKey] == null)
            {
                var returlUrl = Url.Action("Cover", routeValues: new { id });
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returlUrl });
            }

            return View("Index");
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

            TempData[CapchaCompletedKey] = true;
            return Redirect(returnUrl);
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

            TempData[CapchaCompletedKey] = true;
            return this.Redirect(returnUrl);
        }

        public ActionResult OutdatedBrowser()
        {
            return View();
        }
    }
}