﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
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
            
            return this.View();
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
            return this.View(StatusHelper.GetOnlyActualSurveyStatusViewItems(this.GlobalInfo.IsSupervisor));
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.GlobalInfo.IsSupervisor);
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

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