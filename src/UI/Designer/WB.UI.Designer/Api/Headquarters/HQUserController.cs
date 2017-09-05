using System.Web.Http;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Api.Headquarters
{
    [RoutePrefix("api/hq/user")]
    public class HQUserController : ApiController
    {
        [HttpGet]
        [Route("login")]
        [ApiBasicAuth(onlyAllowedAddresses: true)]
        public void Login() { }

        [HttpGet]
        [ApiBasicAuth(onlyAllowedAddresses: false)]
        [Route("userdetails")]
        public DeploymentUserModel UserDetails()
        {
            var membershipService = ServiceLocator.Current.GetInstance<IMembershipUserService>();
            var user = membershipService.WebUser;
            var roles = Roles.GetRolesForUser(user.UserName);
            return new DeploymentUserModel
            {
                Id = user.UserId,
                Login = user.UserName,
                Roles = roles,
                Email = user.MembershipUser.Email
            };
        }
    }
}
