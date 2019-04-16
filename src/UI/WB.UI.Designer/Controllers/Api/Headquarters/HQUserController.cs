using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Models;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Api.Headquarters
{
    [Route("api/hq/user")]
    public class HQUserController : ControllerBase
    {
        [HttpGet]
        [Route("login")]
        [ApiBasicAuth(onlyAllowedAddresses: true)]
        public void Login() { }

        [HttpGet]
        [ApiBasicAuth(onlyAllowedAddresses: false)]
        [Route("userdetails")]
        public IActionResult UserDetails()
        {
            return Ok(new PortalUserModel
            {
                Id = User.GetId(),
                Login = User.Identity.Name,
                Roles = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray(),
                Email = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value
            });
        }
    }
}
