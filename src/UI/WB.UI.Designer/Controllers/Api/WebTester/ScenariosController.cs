using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [Route("api/webtester/Scenarios")]
    public class ScenariosController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;

        public ScenariosController(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [Route("{id}")]
        [Authorize]
        [QuestionnairePermissions(write: true)]
        [HttpPost]
        public async Task<IActionResult> Post(QuestionnaireRevision id, [FromBody]PostScenarioModel model)
        {
            var existingScenario = await this.dbContext.Scenarios.FindAsync(model.ScenarioId);
            if (existingScenario == null)
            {
                var newScenario = new StoredScenario
                {
                    QuestionnaireId = id.QuestionnaireId, 
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

        [Route("{id}")]
        [Authorize]
        [QuestionnairePermissions]
        [HttpGet]
        public async Task<IActionResult> Get(QuestionnaireRevision id)
        {
            var qId = id.OriginalQuestionnaireId ?? id.QuestionnaireId;

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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetForWebTester(Guid questionnaireId, int scenarioId)
        {
            var tokenQuestionnaire = User.FindFirst(JwtTokenService.QuestionnaireIdClaimType);
            if (tokenQuestionnaire == null || !Guid.TryParse(tokenQuestionnaire.Value, out var tokenQuestionnaireId)
                || tokenQuestionnaireId != questionnaireId)
            {
                return Forbid();
            }

            StoredScenario? scenario = await this.dbContext.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new {Message = "Scenario not found"});

            if (questionnaireId != scenario.QuestionnaireId)
            {
                var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires
                    .FirstOrDefault(a => a.AnonymousQuestionnaireId == questionnaireId 
                                         && a.QuestionnaireId == scenario.QuestionnaireId 
                                         && a.IsActive == true);
                if (anonymousQuestionnaire == null)
                    return Forbid();

                scenario.QuestionnaireId = anonymousQuestionnaire.AnonymousQuestionnaireId;
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
