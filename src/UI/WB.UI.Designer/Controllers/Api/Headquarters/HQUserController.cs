using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/user")]
    [Authorize]
    public class HQUserController : ControllerBase
    {
        [HttpGet]
        [Route("login")]
        [AllowOnlyFromWhitelistIP(onlyAllowedAddresses: true)]
        public void Login() { }

        [HttpGet]
        [AllowOnlyFromWhitelistIP]
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
