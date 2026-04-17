using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    /// <summary>
    /// Proxies scenario list and save requests to Designer via backend-to-backend JWT call.
    /// The browser never contacts Designer directly for scenarios.
    /// </summary>
    [Route("api/ScenariosProxy")]
    public class ScenariosProxyController : ControllerBase
    {
        private readonly IDesignerWebTesterApi designerApi;

        public ScenariosProxyController(IDesignerWebTesterApi designerApi)
        {
            this.designerApi = designerApi ?? throw new ArgumentNullException(nameof(designerApi));
        }

        /// <summary>Returns the list of scenarios for the given questionnaire (proxied from Designer).</summary>
        [HttpGet]
        [Route("{questionnaireId:Guid}")]
        public async Task<IActionResult> GetList(Guid questionnaireId)
        {
            var scenarios = await designerApi.GetScenariosListAsync(questionnaireId.ToString());
            return Ok(scenarios);
        }

        /// <summary>Saves (create or update) a scenario for the given questionnaire (proxied to Designer).</summary>
        [HttpPost]
        [Route("{questionnaireId:Guid}")]
        public async Task<IActionResult> Save(Guid questionnaireId, [FromBody] SaveScenarioRequest model)
        {
            var response = await designerApi.SaveScenarioAsync(questionnaireId.ToString(), model);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);
            return Ok();
        }
    }
}

