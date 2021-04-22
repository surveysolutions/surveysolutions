using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    [Route("api/v1/users")]
    public class UsersTypeaheadController : ControllerBase
    {
        private readonly IUserViewFactory usersFactory;
        private readonly ITeamViewFactory teamViewFactory;
        private readonly IAuthorizedUser authorizedUser;

        public UsersTypeaheadController(IUserViewFactory usersFactory, 
            ITeamViewFactory teamViewFactory,
            IAuthorizedUser authorizedUser)
        {
            this.usersFactory = usersFactory;
            this.teamViewFactory = teamViewFactory;
            this.authorizedUser = authorizedUser;
        }


        [HttpGet]
        [Route("supervisors")]
        public TypeaheadApiView Supervisors(string query, int limit = 10, int offset = 1)
            => ToTypeaheadModel(this.usersFactory.GetUsersByRole(offset, limit, null, query, false, UserRoles.Supervisor));

        [HttpGet]
        [Route("workspaceSupervisors")]
        public TypeaheadApiView WorkspaceSupervisors(string query, string workspace, int limit = 10, int offset = 1)
            => ToTypeaheadModel(this.usersFactory.GetUsersByRole(offset, limit, null, query, false, UserRoles.Supervisor, workspace));

        [HttpGet]
        [AuthorizeByRole(UserRoles.Supervisor)]
        public TypeaheadApiView AsigneeInterviewersBySupervisor(string query, int limit = 10)
        {
            var users = this.teamViewFactory.GetAsigneeInterviewersBySupervisor(pageSize: limit, searchBy: query,
                supervisorId: this.authorizedUser.Id);
            return new TypeaheadApiView(
                1,
                users.Users.Count(),
                users.TotalCountByQuery,
                users.Users.Select(
                    item => new TypeaheadOptionalApiView()
                    {
                        key = item.UserId,
                        value = item.UserName
                    }),
                null);
        }

        private TypeaheadApiView ToTypeaheadModel(UserListView users)
        {
            if (users == null)
                return null;

            return new TypeaheadApiView(
                users.Page,
                users.PageSize,
                users.TotalCount,
                users.Items.Select(
                    item => new TypeaheadOptionalApiView()
                    {
                        key = item.UserId,
                        value = item.UserName
                    }),
                null);
        }
    }
}
