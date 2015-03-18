using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IInterviewersViewFactory interviewersFactory;

        private readonly ITeamUsersAndQuestionnairesFactory
            teamUsersAndQuestionnairesFactory;

        public SurveyController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                IInterviewersViewFactory interviewersFactory,
                                ITeamUsersAndQuestionnairesFactory
                                    teamUsersAndQuestionnairesFactory)
            : base(commandService, provider, logger)
        {
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