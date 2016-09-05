using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Headquarters.Controllers
{
    public class TeamsController : BaseApiController
    {
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";
        private const bool DEFAULT_SHOW_LOCKED = false;

        private readonly IIdentityManager identityManager;
        private readonly ITeamViewFactory teamViewFactory;

        public TeamsController(
            ICommandService commandService,
            IIdentityManager identityManager,
            ILogger logger,
            ITeamViewFactory teamViewFactory)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.teamViewFactory = teamViewFactory;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UsersView AssigneeSupervisorsAndDependentInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.teamViewFactory.GetAssigneeSupervisorsAndDependentInterviewers(pageSize: pageSize, searchBy: query);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public UsersView Supervisors(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool showLocked = DEFAULT_SHOW_LOCKED)
        {
            return this.teamViewFactory.GetAllSupervisors(pageSize: pageSize, searchBy: query, showLocked: showLocked);
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public UsersView AsigneeInterviewersBySupervisor(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.teamViewFactory.GetAsigneeInterviewersBySupervisor(pageSize: pageSize, searchBy: query, supervisorId: this.identityManager.CurrentUserId);
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public UsersView Interviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.teamViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, supervisorId: this.identityManager.CurrentUserId);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public UsersView AllInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.teamViewFactory.GetAllInterviewers(pageSize: pageSize, searchBy: query, onlyActive: true);
        }
    }
}