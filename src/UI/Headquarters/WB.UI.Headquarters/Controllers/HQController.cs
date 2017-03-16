using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ITakeNewInterviewViewFactory takeNewInterviewViewFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;

        public HQController(ICommandService commandService, 
            IAuthorizedUser authorizedUser, 
            ILogger logger,
            ITakeNewInterviewViewFactory takeNewInterviewViewFactory,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IQuestionnaireVersionProvider questionnaireVersionProvider)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
        }

        public ActionResult Index()
        {
            return RedirectToActionPermanent("Interviews");
        }

        public ActionResult Interviews(Guid? questionnaireId)
        {
            if (questionnaireId.HasValue)
            {
                this.Success(
                    $@"{HQ.InterviewWasCreated} <a class=""btn btn-success"" href=""{this.Url.Action("TakeNew", "HQ",
                        new { id = questionnaireId.Value })}""><i class=""icon-plus""></i>{HQ.CreateOneMore}</a>");
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            return this.View(this.Filters());
        }

        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        public ActionResult CloneQuestionnaire(Guid id, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            QuestionnaireBrowseItem questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            if (questionnaireBrowseItem == null)
                return new HttpNotFoundResult(string.Format(HQ.QuestionnaireNotFoundFormat, id.FormatGuid(), version));

            return this.View(new CloneQuestionnaireModel(id, version, questionnaireBrowseItem.Title, questionnaireBrowseItem.AllowCensusMode));
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        public ActionResult CloneQuestionnaire(CloneQuestionnaireModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            try
            {
                var newVersion = this.questionnaireVersionProvider.GetNextVersion(model.Id);
                this.CommandService.Execute(new CloneQuestionnaire(
                    model.Id, model.Version, model.NewTitle, newQuestionnaireVersion:newVersion, userId: this.authorizedUser.Id));
            }
            catch (QuestionnaireException exception)
            {
                this.ModelState.AddModelError<CloneQuestionnaireModel>(x => x.NewTitle, exception.Message);
                return this.View(model);
            }
            catch (Exception exception)
            {
                this.Logger.Error($"Unexpected error occurred while cloning questionnaire (id: {model.Id}, version: {model.Version}).", exception);
                this.Error(QuestionnaireClonning.UnexpectedError);
                return this.View(model);
            }

            this.Success(
                model.NewTitle == model.OriginalTitle
                    ? string.Format(HQ.QuestionnaireClonedFormat, model.OriginalTitle)
                    : string.Format(HQ.QuestionnaireClonedAndRenamedFormat, model.OriginalTitle, model.NewTitle));

            return this.RedirectToAction("Index", "SurveySetup");
        }

        public ActionResult TakeNew(Guid id, long? version)
        {
            Guid key = id;
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(key, version, this.authorizedUser.Id));
            return this.View(model);
        }

        public ActionResult SurveysAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;

            return this.View();
        }

        public ActionResult SupervisorsAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return this.View(usersAndQuestionnaires.Questionnaires);
        }

        public ActionResult MapReport()
        {
            this.ViewBag.ActivePage = MenuItem.MapReport;

            return this.View();
        }

        public ActionResult InterviewsChart()
        {
            this.ViewBag.ActivePage = MenuItem.InterviewsChart;

            return this.View(this.Filters());
        }

        public ActionResult Status()
        {
            this.ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor));
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor);

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return new DocumentFilter
            {
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            };
        }
    }
}