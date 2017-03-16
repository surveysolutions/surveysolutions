using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Controllers
{
    public class TeamsController : BaseApiController
    {
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";
        private const bool DEFAULT_SHOW_LOCKED = false;

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
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisorsAndDependentInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAssigneeSupervisorsAndDependentInterviewers(pageSize: pageSize, searchBy: query);

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public UsersView Supervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
            => this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public UsersView AsigneeInterviewersBySupervisor(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAsigneeInterviewersBySupervisor(pageSize: pageSize, searchBy: query,
                supervisorId: this.authorizedUser.Id);

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public UsersView Interviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query,
                    supervisorId: this.authorizedUser.Id);

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public UsersView AllInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, supervisorId: null, onlyActive: true);


        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public ComboboxModel InterviewersCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            var users = this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, onlyActive: true, supervisorId: null);
            var options = users.Users.Select(x => new ComboboxOptionModel(x.UserId.FormatGuid(), x.UserName)).ToArray();
            return new ComboboxModel(options, users.TotalCountByQuery);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public ComboboxModel SupervisorsCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
        {
            var users = this.userViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);
            var options = users.Users.Select(x => new ComboboxOptionModel(x.UserId.FormatGuid(), x.UserName)).ToArray();
            return new ComboboxModel(options, users.TotalCountByQuery);
        }
    }
}