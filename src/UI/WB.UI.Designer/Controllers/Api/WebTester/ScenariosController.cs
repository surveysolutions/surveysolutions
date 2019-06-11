using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [Route("api/webtester/Scenarios")]
    [Authorize]
    public class ScenariosController : ControllerBase
    {

        [Route("{id:Guid}")]
        public IActionResult Post(Guid id, [FromForm]PostScenarioModel model)
        {
            return Ok();
        }
    }

    public class PostScenarioModel
    {
        public string ScenarioText { get; set; }
    }
}
