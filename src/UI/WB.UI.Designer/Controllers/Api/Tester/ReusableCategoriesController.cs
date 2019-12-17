using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [Authorize]
    [Route("api/v{version:int}/categories")]
    public class ReusableCategoriesController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public ReusableCategoriesController(DesignerDbContext dbContext, IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.dbContext = dbContext; 
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            var categoriesIds = questionnaireView.Source.Categories.Select(x => x.Id).ToList();

            var result = await this.dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == id && categoriesIds.Contains(x.CategoriesId))
                .GroupBy(c => c.CategoriesId)
                .Select(x => new ReusableCategoriesDto
                {
                    Id = x.Key,
                    Options = x.Select(o => new CategoriesItem()
                    {
                        Id = o.Value,
                        ParentId = o.ParentId,
                        Text = o.Text
                    }).ToList()
                }).ToArrayAsync();

            return Ok(result);
        }
    }
}
