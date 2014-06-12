using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;

        private readonly IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
            teamUsersAndQuestionnairesFactory;

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

        public ActionResult Interviews()
        {
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
            return this.View(StatusHelper.GetOnlyActualSurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems();
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(viewerId));

            return new DocumentFilter
            {
                Users =
                    this.interviewersFactory.Load(new InterviewersInputModel(viewerId) { PageSize = int.MaxValue })
                        .Items.Where(u => !u.IsLockedBySupervisor && !u.IsLockedByHQ)
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