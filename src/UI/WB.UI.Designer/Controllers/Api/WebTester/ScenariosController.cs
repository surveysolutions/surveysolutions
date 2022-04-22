using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Scenarios;
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
        [QuestionnairePermissions(true)]
        [HttpPost]
        public async Task<IActionResult> Post(Guid id, [FromBody]PostScenarioModel model)
        {
            var existingScenario = await this.dbContext.Scenarios.FindAsync(model.ScenarioId);
            if (existingScenario == null)
            {
                var newScenario = new StoredScenario
                {
                    QuestionnaireId = id, 
                    Steps = model.ScenarioText ?? "", 
                    Title = model.ScenarioTitle ?? "New scenario"
                };
                await this.dbContext.Scenarios.AddAsync(newScenario);
            }
            else
            {
                existingScenario.Steps = model.ScenarioText ?? "";
            }
            
            await this.dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("{id:Guid}")]
        [QuestionnairePermissions]
        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var scenarios = await this.dbContext.Scenarios.Where(x => x.QuestionnaireId == id)
                                                          .OrderBy(x => x.Title)
                                                          .Select(x => new
                                                          {
                                                              x.Title,
                                                              x.Id
                                                          }).ToListAsync();

            return Ok(scenarios);
        }

        [Route("{token}/{scenarioId:int}")]
        public async Task<IActionResult> Get(string token, int scenarioId)
        {
            var questionnaire = this.webTesterService.GetQuestionnaire(token);
            if (questionnaire == null) return Forbid("Token expired");

            StoredScenario? scenario = await this.dbContext.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new {Message = "Scenario not found"});
            if (questionnaire != scenario.QuestionnaireId)
                return Forbid("Scenario from other questionnaire");

            return Ok(scenario.Steps);
        }
    }

    public class PostScenarioModel
    {
        public string? ScenarioText { get; set; }
        public int? ScenarioId { get; set; }
        public string? ScenarioTitle { get; set; }
    }
}
