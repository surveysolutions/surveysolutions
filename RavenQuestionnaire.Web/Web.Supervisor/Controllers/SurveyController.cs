using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.Interviews;
using Core.Supervisor.Views.Status;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Core.Supervisor.Views.User;
using Core.Supervisor.Views.UsersAndQuestionnaires;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;
        private readonly IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory;
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView> teamUsersAndQuestionnairesFactory;

        public SurveyController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory,
            IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory, 
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory,
            IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView> teamUsersAndQuestionnairesFactory)
            : base(commandService, provider, logger)
        {
            this.surveyUsersViewFactory = surveyUsersViewFactory;
            this.summaryTemplatesViewFactory = summaryTemplatesViewFactory;
            this.interviewersFactory = interviewersFactory;
            this.teamUsersAndQuestionnairesFactory = teamUsersAndQuestionnairesFactory;
        }

        public ActionResult Index()
        {
            ViewBag.ActivePage = MenuItem.Surveys;
            return
                this.View(
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                        ViewerStatus.Supervisor)).Items);
        }

        public ActionResult Interviews()
        {
            ViewBag.ActivePage = MenuItem.Docs;
            return this.View(Filters());
        }

        public ActionResult GotoBrowser()
        {
            return this.RedirectToAction("Index");
        }

        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Summary;
            return this.View(this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(
                this.GlobalInfo.GetCurrentUser().Id, ViewerStatus.Supervisor)).Items);
        }

        public ActionResult Status()
        {
            ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.SurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            var statuses = StatusHelper.SurveyStatusViewItems();
            var viewerId = this.GlobalInfo.GetCurrentUser().Id;

            var usersAndQuestionnaires = teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(viewerId));

            return new DocumentFilter
            {
                Users = this.interviewersFactory.Load(new InterviewersInputModel(viewerId){PageSize = int.MaxValue}).Items.Where(u => !u.IsLocked).Select(u => new SurveyUsersViewItem()
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