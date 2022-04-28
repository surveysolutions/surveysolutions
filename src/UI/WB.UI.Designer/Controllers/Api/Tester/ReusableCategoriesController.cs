using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [AuthorizeOrAnonymousQuestionnaire]
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

        [QuestionnairePermissions]
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(QuestionnaireRevision id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();
            var categoriesIds = questionnaireView.Source.Categories.Select(x => x.Id).ToList();

            var questionnaireId = id.OriginalQuestionnaireId ?? id.QuestionnaireId;

            return Ok(categoriesIds.Select(categoriesId =>
            {
                return new ReusableCategoriesDto
                {
                    Id = categoriesId,
                    Options = this.categoriesService
                        .GetCategoriesById(questionnaireId, categoriesId).ToList()
                };
            }));
        }
    }
}
