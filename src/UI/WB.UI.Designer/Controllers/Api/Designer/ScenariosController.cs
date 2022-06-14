using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [ResponseCache(NoStore = true)]
    [Route("api/questionnaire/{questionnaireId:Guid}/scenarios")]
    public class ScenariosController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireViewFactory viewFactory;

        public ScenariosController(DesignerDbContext dbContext, IQuestionnaireViewFactory viewFactory)
        {
            this.dbContext = dbContext;
            this.viewFactory = viewFactory;
        }

        public class UpdateScenarioModel
        {
            public string? Title { get; set; }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateScenario(Guid questionnaireId, int id, [FromBody] UpdateScenarioModel model)
        {
            var hasUserAccess = viewFactory.HasUserAccessToRevertQuestionnaire(questionnaireId, User.GetId());
            if (!hasUserAccess)
                return Forbid();

            var scenario = await dbContext.Scenarios.FindAsync(id);
            if (scenario == null || scenario.QuestionnaireId != questionnaireId)
                return NotFound();

            scenario.Title = model.Title ?? "";

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("{scenarioId:int}")]
        [HttpPatch]
        public async Task<IActionResult> Patch(Guid questionnaireId, int scenarioId, [FromBody]UpdateStepsModel content)
        {
            var hasUserAccess = viewFactory.HasUserAccessToRevertQuestionnaire(questionnaireId, User.GetId());
            if (!hasUserAccess)
                return Forbid();

            StoredScenario? scenario = await this.dbContext.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new { Message = "Scenario not found" });

            scenario.Steps = content.Steps ?? "";
            await this.dbContext.SaveChangesAsync();

            return Ok(scenario.Steps);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteScenario(Guid questionnaireId, int id)
        {
            var hasUserAccess = viewFactory.HasUserAccessToRevertQuestionnaire(questionnaireId, User.GetId());
            if (!hasUserAccess)
                return Forbid();

            var scenario = await dbContext.Scenarios.FindAsync(id);
            if (scenario == null || scenario.QuestionnaireId != questionnaireId)
                return NotFound();

            dbContext.Scenarios.Remove(scenario);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("{id:int}")]
        public async Task<IActionResult> Get(Guid questionnaireId, int id)
        {
            var hasUserAccess = User.IsAdmin() || viewFactory.HasUserAccessToQuestionnaire(questionnaireId, User.GetIdOrNull());
            if (!hasUserAccess)
                return Forbid();

            var scenario = await dbContext.Scenarios.FindAsync(id);
            if (scenario == null)
                return NotFound();
                    
            if (scenario.QuestionnaireId != questionnaireId)
            {
                var anonymousQuestionnaire = this.dbContext.AnonymousQuestionnaires
                    .FirstOrDefault(a => a.AnonymousQuestionnaireId == questionnaireId && a.IsActive == true);
                if (anonymousQuestionnaire == null)
                    return NotFound();
            }
            
            return Ok(scenario.Steps);
        }
    }
}
