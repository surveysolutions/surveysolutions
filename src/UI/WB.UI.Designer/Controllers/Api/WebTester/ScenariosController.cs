using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [Route("api/webtester/Scenarios")]
    public class ScenariosController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;
        private readonly IWebTesterService webTesterService;

        public ScenariosController(DesignerDbContext dbContext, IWebTesterService webTesterService)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.webTesterService = webTesterService ?? throw new ArgumentNullException(nameof(webTesterService));
        }

        [Route("{id:Guid}")]
        [Authorize]
        [QuestionnairePermissions]
        public async Task<IActionResult> Post(Guid id, [FromForm]PostScenarioModel model)
        {
            var newScenario = new StoredScenario();
            newScenario.QuestionnaireId = id;
            newScenario.Steps = model.ScenarioText;
            newScenario.Title = "";

            await this.dbContext.Scenarios.AddAsync(newScenario);
            await this.dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Questionnaire", new {id = id.FormatGuid()});
        }

        [Route("{token}/{scenarioId:int}")]
        public async Task<IActionResult> Get(string token, int scenarioId)
        {
            var questionnaire = this.webTesterService.GetQuestionnaire(token);
            if (questionnaire == null) return Forbid("Token expired");

            StoredScenario scenario = await this.dbContext.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new {Message = "Scenario not found"});
            if (questionnaire != scenario.QuestionnaireId)
                return Forbid("Scenario from other questionnaire");

            return Ok(scenario.Steps);
        }
    }

    public class PostScenarioModel
    {
        public string ScenarioText { get; set; }
    }
}
