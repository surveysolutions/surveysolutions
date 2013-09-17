using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.Survey;
using Core.Supervisor.Views.UsersAndQuestionnaires;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;

        private readonly IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
            teamUsersAndQuestionnairesFactory;

        private readonly InterviewStatus[] interviewStatusesToSkip =
        {
            InterviewStatus.Created,
            InterviewStatus.ReadyForInterview,
            InterviewStatus.Restarted,
            InterviewStatus.SentToCapi,
            InterviewStatus.Deleted
        };

        public SurveyController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory,
                                IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory,
                                IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
                                    teamUsersAndQuestionnairesFactory)
            : base(commandService, provider, logger)
        {
            this.surveyUsersViewFactory = surveyUsersViewFactory;
            this.interviewersFactory = interviewersFactory;
            this.teamUsersAndQuestionnairesFactory = teamUsersAndQuestionnairesFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;
            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(this.GlobalInfo.GetCurrentUser().Id));
            return this.View(usersAndQuestionnaires.Users);
        }

        public ActionResult Interviews(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                this.Success(string.Format(@"Status was successfully changed. Interview is {0}", status));
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            UserLight currentUser = this.GlobalInfo.GetCurrentUser();
            this.ViewBag.CurrentUser = new UsersViewItem { UserId = currentUser.Id, UserName = currentUser.Name };
            return this.View(this.Filters());
        }

        public ActionResult TeamMembersAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;
            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(this.GlobalInfo.GetCurrentUser().Id));
            return this.View(usersAndQuestionnaires.Questionnaires);
        }

        public ActionResult Status()
        {
            this.ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.SurveyStatusViewItems(skipStatuses: this.interviewStatusesToSkip));
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.SurveyStatusViewItems(skipStatuses: this.interviewStatusesToSkip);
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(viewerId));

            return new DocumentFilter
            {
                Users =
                    this.interviewersFactory.Load(new InterviewersInputModel(viewerId) { PageSize = int.MaxValue })
                        .Items.Where(u => !u.IsLocked)
                        .Select(u => new UsersViewItem
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