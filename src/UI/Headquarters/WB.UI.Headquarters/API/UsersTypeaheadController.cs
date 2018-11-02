using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
 
namespace WB.UI.Headquarters.API
{
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
    [RoutePrefix("api/v1/users")]
    public class UsersTypeaheadController : ApiController
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
