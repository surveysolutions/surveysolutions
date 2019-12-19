using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    [Route("api/v1/users")]
    public class UsersTypeaheadController : ControllerBase
    {
        private readonly IUserViewFactory usersFactory;

        public UsersTypeaheadController(IUserViewFactory usersFactory)
        {
            this.usersFactory = usersFactory;
        }


        [HttpGet]
        [Route("supervisors")]
        public TypeaheadApiView Supervisors(string query, int limit = 10, int offset = 1)
            => ToTypeaheadModel(this.usersFactory.GetUsersByRole(offset, limit, null, query, false, UserRoles.Supervisor));

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
