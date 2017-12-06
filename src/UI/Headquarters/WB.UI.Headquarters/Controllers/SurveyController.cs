using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IAuthorizedUser authorizedUser;

        private readonly ITeamUsersAndQuestionnairesFactory
            teamUsersAndQuestionnairesFactory;

        public SurveyController(
            ICommandService commandService, 
            IAuthorizedUser authorizedUser, 
            ILogger logger,
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.teamUsersAndQuestionnairesFactory = teamUsersAndQuestionnairesFactory;
        }

        public ActionResult Interviews()
        {
            this.ViewBag.ActivePage = MenuItem.Docs;
            this.ViewBag.CurrentUser = new UsersViewItem
            {
                UserId = this.authorizedUser.Id,
                UserName = this.authorizedUser.UserName
            };
            return this.View(this.Filters());
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor);
            Guid viewerId = this.authorizedUser.Id;

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