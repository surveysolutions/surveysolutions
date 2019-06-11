using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [Route("api/webtester/Scenarios")]
    [Authorize]
    [QuestionnairePermissions]
    public class ScenariosController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;

        public ScenariosController(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [Route("{id:Guid}")]
        public async Task<IActionResult> Post(Guid id, [FromForm]PostScenarioModel model)
        {
            var newScenario = new StoredScenario();
            newScenario.QuestionnaireId = id;
            newScenario.Steps = model.ScenarioText;
            newScenario.Title = "";

            await this.dbContext.Scenarios.AddAsync(newScenario);
            await this.dbContext.SaveChangesAsync();

            return Ok();
        }
    }

    public class PostScenarioModel
    {
        public string ScenarioText { get; set; }
    }
}
