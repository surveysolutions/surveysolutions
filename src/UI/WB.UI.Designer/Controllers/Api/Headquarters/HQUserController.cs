using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Designer.Code.Attributes;
using WB.Core.BoundedContexts.Designer;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/user")]
    [Authorize]
    public class HQUserController : ControllerBase
    {
        [HttpGet]
        [Route("login")]
        public void Login() { }

        [HttpGet]
        [Route("userdetails")]
        public IActionResult? UserDetails()
        {
            if (User == null)
                return null;

            return Ok(new PortalUserModel(
                id : User.GetId(),
                login : User.Identity!.Name,
                roles : User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray(),
                email : User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value
            ));
        }
    }
}
