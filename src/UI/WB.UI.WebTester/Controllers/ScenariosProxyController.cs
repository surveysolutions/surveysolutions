using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    /// <summary>
    /// Proxies scenario list and save requests to Designer via backend-to-backend JWT call.
    /// The browser never contacts Designer directly for scenarios.
    /// Routes are interview-scoped (keyed by <c>interviewId</c>) so that each tab's proxy
    /// traffic resolves to its own JWT — concurrent tabs no longer interfere.
    /// </summary>
    [Route("api/ScenariosProxy")]
    [WebTesterSessionAuthorize]
    public class ScenariosProxyController : ControllerBase
    {
        private readonly IDesignerWebTesterApi designerApi;
        private readonly IWebTesterSessionService sessionService;

        public ScenariosProxyController(
            IDesignerWebTesterApi designerApi,
            IWebTesterSessionService sessionService)
        {
            this.designerApi = designerApi ?? throw new ArgumentNullException(nameof(designerApi));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        /// <summary>Returns the list of scenarios for the given interview's questionnaire (proxied from Designer).</summary>
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetList(Guid id)
        {
            var questionnaireId = sessionService.GetQuestionnaireId(HttpContext.Session, id);
            if (questionnaireId == null)
                return NotFound(new { error = "Interview session expired or not authorized" });

            var response = await designerApi.GetScenariosListAsync(questionnaireId.Value.ToString());
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);
            return Ok(response.Content);
        }

        /// <summary>Saves (create or update) a scenario for the given interview's questionnaire (proxied to Designer).</summary>
        [HttpPost]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Save(Guid id, [FromBody] SaveScenarioRequest? model)
        {
            if (model == null)
                return BadRequest(new { error = "Request body is required" });

            var questionnaireId = sessionService.GetQuestionnaireId(HttpContext.Session, id);
            if (questionnaireId == null)
                return NotFound(new { error = "Interview session expired or not authorized" });

            var response = await designerApi.SaveScenarioAsync(questionnaireId.Value.ToString(), model);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);
            return Ok();
        }
    }
}
