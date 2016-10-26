using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IIdentityManager identityManager;

        private readonly ITeamUsersAndQuestionnairesFactory
            teamUsersAndQuestionnairesFactory;

        public SurveyController(
            ICommandService commandService, 
            IIdentityManager identityManager, 
            ILogger logger,
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.teamUsersAndQuestionnairesFactory = teamUsersAndQuestionnairesFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;
            
            return this.View();
        }

        public ActionResult Interviews()
        {
            this.ViewBag.ActivePage = MenuItem.Docs;
            this.ViewBag.CurrentUser = new UsersViewItem
            {
                UserId = this.identityManager.CurrentUserId,
                UserName = this.identityManager.CurrentUserName
            };
            return this.View(this.Filters());
        }

        public ActionResult TeamMembersAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;
            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(this.identityManager.CurrentUserId));
            return this.View(usersAndQuestionnaires.Questionnaires);
        }

        public ActionResult Status()
        {
            this.ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.GetOnlyActualSurveyStatusViewItems(this.identityManager.IsCurrentUserSupervisor));
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.identityManager.IsCurrentUserSupervisor);
            Guid viewerId = this.identityManager.CurrentUserId;

            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(viewerId));

            return new DocumentFilter
            {
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            };
        }
    }
}