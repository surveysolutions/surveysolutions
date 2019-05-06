using System.Net;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [Route("api/v{version:int}/user")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("login")]
        public IActionResult Login(int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode((int) HttpStatusCode.UpgradeRequired);
            return Ok();
        }
    }
}
