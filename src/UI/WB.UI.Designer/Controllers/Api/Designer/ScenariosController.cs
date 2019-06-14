using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize(Roles =  "Administrator")]
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
            public string Title { get; set; }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateScenario(Guid questionnaireId, int id, [FromBody] UpdateScenarioModel model)
        {
            var hasUserAccess = viewFactory.HasUserAccessToQuestionnaire(questionnaireId, User.GetId());
            if (!hasUserAccess)
                return Forbid();

            var scenario = await dbContext.Scenarios.FindAsync(id);
            if (scenario == null || scenario.QuestionnaireId != questionnaireId)
                return NotFound();

            scenario.Title = model.Title;

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteScenario(Guid questionnaireId, int id)
        {
            var hasUserAccess = viewFactory.HasUserAccessToQuestionnaire(questionnaireId, User.GetId());
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
            var hasUserAccess = viewFactory.HasUserAccessToQuestionnaire(questionnaireId, User.GetId());
            if (!hasUserAccess)
                return Forbid();

            var scenario = await dbContext.Scenarios.FindAsync(id);
            if (scenario == null || scenario.QuestionnaireId != questionnaireId)
                return NotFound();

            return Ok(scenario.Steps);
        }
    }
}
