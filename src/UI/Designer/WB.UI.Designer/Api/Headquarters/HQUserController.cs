using System.Web.Http;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Headquarters
{
    [ApiBasicAuth(onlyAllowedAddresses: true)]
    [RoutePrefix("api/hq/user")]
    public class HQUserController : ApiController
    {
        [HttpGet]
        [Route("login")]
        public void Login() { }
    }
}
