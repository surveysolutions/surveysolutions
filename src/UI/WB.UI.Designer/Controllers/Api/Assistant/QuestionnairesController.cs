using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.Assistant
{
    //TODO:Improve Authorization, Token based
    //[Authorize]
    [Route("api/v1/assistant/questionnaires")]
    public class QuestionnairesController : Controller
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireDocumentTransformer questionnaireDocumentTransformer;
        private readonly IReusableCategoriesService reusableCategoriesService;
        
        private readonly ISerializer serializer;

        public QuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireDocumentTransformer questionnaireDocumentTransformer, 
            IReusableCategoriesService reusableCategoriesService,
            ISerializer serializer)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireDocumentTransformer = questionnaireDocumentTransformer;
            this.serializer = serializer;
            this.reusableCategoriesService = reusableCategoriesService;
        }
        
        //[QuestionnairePermissions]
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(QuestionnaireRevision id)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            var questionnaire = questionnaireView.GetClientReadyDocument();
            questionnaireDocumentTransformer.TransformInPlace(questionnaire);
            
            // Clear macros as they are not needed for assistant
            questionnaire.Macros = new Dictionary<Guid, Macro>();

            var response = this.serializer.Serialize(questionnaire);

            return Content(response, MediaTypeNames.Application.Json);
        }
        
        [HttpGet]
        [Route("{id}/category/{categoryId}")]
        public IActionResult GetCategory(Guid id, Guid categoryId)
        {
            var categories = this.reusableCategoriesService.GetCategoriesById(id, categoryId);
            if (categories == null) return NotFound();

            var result = JsonConvert.SerializeObject(categories, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });

            return Content(result, "application/json", Encoding.UTF8);
        }
        
        [HttpGet]
        [Route("{id}/categories")]
        public IActionResult GetCategories(QuestionnaireRevision id)
        {
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
                    Options = this.reusableCategoriesService
                        .GetCategoriesById(questionnaireId, categoriesId).ToList()
                };
            }));
        }
    }
}
