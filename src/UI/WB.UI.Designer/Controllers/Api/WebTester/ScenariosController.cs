using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [ApiController]
    [Route("api/webtester/Scenarios")]
    [Authorize(AuthenticationSchemes = DelegatedTokenService.DelegatedScheme)]
    public class ScenariosController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;

        public ScenariosController(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [Route("{id:Guid}")]
        [QuestionnairePermissions(write: true)]
        [HttpPost]
        public async Task<IActionResult> Post(Guid id, [FromBody]PostScenarioModel model)
        {
            if (!User.HasMatchingQuestionnaireId(id))
                return Forbid();

            // Resolve the effective questionnaire ID (anonymous questionnaire mapping).
            var effectiveId = id;
            var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires
                .FirstOrDefault(a => a.AnonymousQuestionnaireId == id && a.IsActive == true);
            if (anonymousQuestionnaire != null)
                effectiveId = anonymousQuestionnaire.QuestionnaireId;

            var existingScenario = model.ScenarioId.HasValue
                ? await this.dbContext.Scenarios.FindAsync(model.ScenarioId.Value)
                : null;

            if (existingScenario == null)
            {
                var newScenario = new StoredScenario
                {
                    QuestionnaireId = effectiveId,
                    Steps = model.ScenarioText ?? "",
                    Title = model.ScenarioTitle ?? "New scenario"
                };
                await this.dbContext.Scenarios.AddAsync(newScenario);
            }
            else
            {
                // IDOR guard: the scenario must belong to the questionnaire from the route.
                // Without this check a delegated token for questionnaire A could overwrite
                // a scenario from questionnaire B by supplying its ScenarioId.
                if (existingScenario.QuestionnaireId != effectiveId)
                    return Forbid();

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
            if (!User.HasMatchingQuestionnaireId(id))
                return Forbid();

            var qId = id;
            var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires
                .FirstOrDefault(a => a.AnonymousQuestionnaireId == id && a.IsActive == true);
            if (anonymousQuestionnaire != null)
                qId = anonymousQuestionnaire.QuestionnaireId;

            var scenarios = await this.dbContext.Scenarios.Where(x => x.QuestionnaireId == qId)
                                                          .OrderBy(x => x.Title)
                                                          .Select(x => new
                                                          {
                                                              x.Title,
                                                              x.Id
                                                          }).ToListAsync();

            return Ok(scenarios);
        }

        [Route("{questionnaireId:Guid}/{scenarioId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetForWebTester(Guid questionnaireId, int scenarioId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            StoredScenario? scenario = await this.dbContext.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new { Message = "Scenario not found" });

            if (questionnaireId != scenario.QuestionnaireId)
            {
                var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires
                    .FirstOrDefault(a => a.AnonymousQuestionnaireId == questionnaireId
                                         && a.QuestionnaireId == scenario.QuestionnaireId
                                         && a.IsActive == true);
                if (anonymousQuestionnaire == null)
                    return Forbid();
            }

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
