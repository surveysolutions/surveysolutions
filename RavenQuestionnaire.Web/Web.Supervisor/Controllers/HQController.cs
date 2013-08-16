using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Assign;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Core.Supervisor.Views.TakeNew;
using Core.Supervisor.Views.User;
using Core.Supervisor.Views.UsersAndQuestionnaires;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Implementation.SampleRecordsAccessors;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.SampleImport;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory;
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireItemFactory;
        private readonly ISampleImportService sampleImportService;
        private readonly IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> userListViewFactory;


        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
                            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireItemFactory,
                            IViewFactory<UserListViewInputModel, UserListView> userListViewFactory,
                            IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory,
                            IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory,
                            IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory,
                            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
                            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory,
                            ISampleImportService sampleImportService,
                            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
                                allUsersAndQuestionnairesFactory)
            : base(commandService, provider, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireItemFactory = questionnaireItemFactory;
            this.userListViewFactory = userListViewFactory;
            this.assignSurveyViewFactory = assignSurveyViewFactory;
            this.surveyUsersViewFactory = surveyUsersViewFactory;
            this.summaryTemplatesViewFactory = summaryTemplatesViewFactory;
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.sampleImportService = sampleImportService;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.supervisorsFactory = supervisorsFactory;
        }

        public ActionResult Index()
        {
            var model = new HQDashboardModel
                {
                    Questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()),
                    Teams = this.userListViewFactory.Load(new UserListViewInputModel {Role = UserRoles.Supervisor})
                };
            return this.View(model);
        }

        public ActionResult Interviews(Guid? questionnaireId)
        {
            if (questionnaireId.HasValue)
            {
                this.Success(
                    string.Format(
                        @"Interview was successfully created. <a class=""btn btn-success"" href=""{0}""><i class=""icon-plus""></i> Create one more?</a>",
                        this.Url.Action("TakeNew", "HQ", new {id = questionnaireId.Value})));
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            return this.View(this.Filters());
        }

        public ActionResult BatchUpload(Guid id)
        {
            QuestionnaireBrowseItem model = this.questionnaireItemFactory.Load(new QuestionnaireItemInputModel(id));
            return this.View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportSample(Guid id, HttpPostedFileBase uploadFile)
        {
            this.ViewBag.ImportId = this.sampleImportService.ImportSampleAsync(id, new CsvSampleRecordsAccessor(uploadFile.InputStream));
            QuestionnaireBrowseItem model = this.questionnaireItemFactory.Load(new QuestionnaireItemInputModel(id));
            return this.View(model);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ImportResult(Guid id)
        {
            ImportResult result = this.sampleImportService.GetImportStatus(id);
            if (result.IsCompleted && result.IsSuccessed)
            {
                this.ViewBag.SupervisorList =
                    this.supervisorsFactory.Load(new UserListViewInputModel {Role = UserRoles.Supervisor, PageSize = int.MaxValue}).Items;
            }
            return this.PartialView(result);
        }

        public ActionResult CreateSample(Guid id, Guid responsibleSupervisor)
        {
            this.sampleImportService.CreateSample(id, this.GlobalInfo.GetCurrentUser().Id, responsibleSupervisor);
            return this.RedirectToAction("SampleCreationResult", new {id});
        }

        public ActionResult SampleCreationResult(Guid id)
        {
            SampleCreationStatus result = this.sampleImportService.GetSampleCreationStatus(id);
            return this.View(result);
        }

        public JsonResult GetSampleCreationStatus(Guid id)
        {
            return this.Json(this.sampleImportService.GetSampleCreationStatus(id));
        }

        public ActionResult TakeNew(Guid id)
        {
            Guid key = id;
            UserLight user = this.GlobalInfo.GetCurrentUser();
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(key, user.Id));
            model.CurrentUser = user;
            return this.View(model);
        }

        public ActionResult SurveysAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;
            return
                this.View(
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                                                                                   ViewerStatus.Headquarter)).Items);
        }

        public ActionResult Assign(Guid id)
        {
            UserLight user = this.GlobalInfo.GetCurrentUser();
            AssignSurveyView model = this.assignSurveyViewFactory.Load(new AssignSurveyInputModel(id, user.Id));
            return this.View(model);
        }

        public ActionResult SupervisorsAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;
            return this.View(this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(
                                                                       this.GlobalInfo.GetCurrentUser().Id, ViewerStatus.Headquarter)).Items);
        }

        public ActionResult Status()
        {
            this.ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.SurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.SurveyStatusViewItems();

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return new DocumentFilter
                {
                    Users =
                        this.supervisorsFactory.Load(new UserListViewInputModel {PageSize = int.MaxValue})
                            .Items.Where(u => !u.IsLocked)
                            .Select(u => new SurveyUsersViewItem
                                {
                                    UserId = u.UserId,
                                    UserName = u.UserName
                                }),
                    Responsibles = usersAndQuestionnaires.Users,
                    Templates = usersAndQuestionnaires.Questionnaires,
                    Statuses = statuses
                };
        }
    }
}