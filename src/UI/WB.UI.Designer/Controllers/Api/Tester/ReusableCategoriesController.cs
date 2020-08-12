using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [Authorize]
    [Route("api/v{version:int}/categories")]
    public class ReusableCategoriesController : ControllerBase
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly ICategoriesService categoriesService;

        public ReusableCategoriesController(IQuestionnaireViewFactory questionnaireViewFactory, ICategoriesService categoriesService)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.categoriesService = categoriesService;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public IActionResult Get(Guid id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return NotFound();
            var categoriesIds = questionnaireView.Source.Categories.Select(x => x.Id).ToList();

            return Ok(categoriesIds.Select(categoriesId => new ReusableCategoriesDto
            {
                Id = categoriesId,
                Options = this.categoriesService.GetCategoriesById(id, categoriesId).ToList()
            }));
        }
    }
}
