using System;
using System.Web.Http;
using System.Web.Security;
using WB.UI.Designer.Api.Attributes;

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
        public MembershipUser UserDetails(Guid id)
        {
            return Membership.GetUser(id, false);
        }
    }
}
