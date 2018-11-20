using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Models.User;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [ApiNoCache]
    public class TeamsController : BaseApiController
    {
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";
        private const bool DEFAULT_SHOW_LOCKED = false;
        private const bool DEFAULT_SHOW_ARCHIVED = false;

        private readonly IAuthorizedUser authorizedUser;
        private readonly ITeamViewFactory teamViewFactory;
        private readonly IUserViewFactory userViewFactory;

        public TeamsController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            ITeamViewFactory teamViewFactory,
            IUserViewFactory userViewFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.teamViewFactory = teamViewFactory;
            this.userViewFactory = userViewFactory;
        }

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAssigneeSupervisors(pageSize: pageSize, searchBy: query);

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisorsAndDependentInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAssigneeSupervisorsAndDependentInterviewers(pageSize: pageSize, searchBy: query);

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        public UsersView Supervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
            => this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        public ResponsibleView Responsibles(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
            => this.userViewFactory.GetAllResponsibles(pageSize: pageSize, searchBy: query, showLocked: showLocked);

        [HttpGet]
        [AuthorizeOr403(Roles = "Supervisor")]
        public UsersView AsigneeInterviewersBySupervisor(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAsigneeInterviewersBySupervisor(pageSize: pageSize, searchBy: query,
                supervisorId: this.authorizedUser.Id);

        [HttpGet]
        [AuthorizeOr403(Roles = "Supervisor")]
        public UsersView Interviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query,
                    supervisorId: this.authorizedUser.Id);

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        public UsersView AllInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, supervisorId: null);


        [HttpGet]
        [Authorize]
        [CamelCase]
        public ResponsibleComboboxModel InterviewersCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED, bool showArchived = DEFAULT_SHOW_ARCHIVED)
        {
            bool? isNeedShowActiveAndArchivedInterviewers = showArchived ? (bool?) null : false;
            var supervisorId = this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?)null;
            var users = this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, supervisorId: supervisorId, showLocked: showLocked, archived: isNeedShowActiveAndArchivedInterviewers);
            var options = users.Users.Select(x => new ResponsibleComboboxOptionModel(x.UserId.FormatGuid(), x.UserName, x.IconClass)).ToArray();
            return new ResponsibleComboboxModel(options, users.TotalCountByQuery);
        }

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public ResponsibleComboboxModel ResponsiblesCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED, bool showArchived = DEFAULT_SHOW_ARCHIVED)
        {
            var users = this.userViewFactory.GetAllResponsibles(pageSize: pageSize, searchBy: query, showLocked: showLocked, showArchived: showArchived);
            var options = users.Users.Select(x => new ResponsibleComboboxOptionModel(x.ResponsibleId.FormatGuid(), x.UserName, x.IconClass)).ToArray();
            return new ResponsibleComboboxModel(options, users.TotalCountByQuery);
        }

        [HttpGet]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public ComboboxModel SupervisorsCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
        {
            var users = this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);
            var options = users.Users.Select(x => new ComboboxOptionModel(x.UserId.FormatGuid(), x.UserName)).ToArray();
            return new ComboboxModel(options, users.TotalCountByQuery);
        }
    }
}
