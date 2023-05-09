using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Models.User;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    public class TeamsController : ControllerBase
    {
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";
        private const bool DEFAULT_SHOW_LOCKED = false;
        private const bool DEFAULT_SHOW_ARCHIVED = false;
        private const bool DEFAULT_EXCLUDE_HEADQUARTERS = false;

        private readonly IAuthorizedUser authorizedUser;
        private readonly ITeamViewFactory teamViewFactory;
        private readonly IUserViewFactory userViewFactory;

        public TeamsController(
            IAuthorizedUser authorizedUser,
            ITeamViewFactory teamViewFactory,
            IUserViewFactory userViewFactory)
        {
            this.authorizedUser = authorizedUser;
            this.teamViewFactory = teamViewFactory;
            this.userViewFactory = userViewFactory;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAssigneeSupervisors(pageSize: pageSize, searchBy: query);

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisorsAndDependentInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAssigneeSupervisorsAndDependentInterviewers(pageSize: pageSize, searchBy: query);

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public UsersView Supervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
            => this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ResponsibleView Responsibles(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
            => this.userViewFactory.GetAllResponsibles(pageSize: pageSize, searchBy: query, showLocked: showLocked);

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public UsersView AsigneeInterviewersBySupervisor(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAsigneeInterviewersBySupervisor(pageSize: pageSize, searchBy: query,
                supervisorId: this.authorizedUser.Id);

        [HttpGet]
        [Authorize]
        public ResponsibleComboboxModel InterviewersCombobox(string query = DEFAULTEMPTYQUERY, 
            int pageSize = DEFAULTPAGESIZE, 
            bool showLocked = DEFAULT_SHOW_LOCKED, 
            bool showArchived = DEFAULT_SHOW_ARCHIVED,
            bool onlyEqual = false)
        {
            bool? isNeedShowActiveAndArchivedInterviewers = showArchived ? (bool?) null : false;
            var supervisorId = this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?)null;
            var filterRule = onlyEqual ? QueryFilterRule.Equals : QueryFilterRule.Contains; 
            var users = this.userViewFactory.GetTeamResponsibles(pageSize: pageSize, searchBy: query, supervisorId: supervisorId, showLocked: showLocked, archived: isNeedShowActiveAndArchivedInterviewers, filterRule);
            var options = users.Users.Select(x => new ResponsibleComboboxOptionModel(x.UserId.FormatGuid(), x.UserName, x.IconClass)).ToArray();
            return new ResponsibleComboboxModel(options, users.TotalCountByQuery);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ResponsibleComboboxModel ResponsiblesCombobox(string query = DEFAULTEMPTYQUERY, 
            int pageSize = DEFAULTPAGESIZE, 
            bool showLocked = DEFAULT_SHOW_LOCKED, 
            bool showArchived = DEFAULT_SHOW_ARCHIVED,
            bool excludeHeadquarters = DEFAULT_EXCLUDE_HEADQUARTERS)
        {
            var users = this.userViewFactory.GetAllResponsibles(pageSize: pageSize, searchBy: query, 
                showLocked: showLocked, showArchived: showArchived, excludeHeadquarters: excludeHeadquarters);
            var options = users.Users.Select(x => new ResponsibleComboboxOptionModel(x.ResponsibleId.FormatGuid(), x.UserName, x.IconClass)).ToArray();
            return new ResponsibleComboboxModel(options, users.TotalCountByQuery);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ComboboxModel SupervisorsCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
        {
            var users = this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);
            var options = users.Users.Select(x => new ComboboxOptionModel(x.UserId.FormatGuid(), x.UserName)).ToArray();
            return new ComboboxModel(options, users.TotalCountByQuery);
        }
    }
}
