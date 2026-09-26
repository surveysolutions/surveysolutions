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
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.Assistant
{
    [Authorize]
    [QuestionnairePermissions]
    [Route("api/v1/assistant/questionnaires")]
    public class QuestionnairesController : Controller
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireDocumentTransformer questionnaireDocumentTransformer;
        private readonly IReusableCategoriesService reusableCategoriesService;
        private readonly ILookupTableService lookupTableService;
        
        private readonly ISerializer serializer;

        public QuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireDocumentTransformer questionnaireDocumentTransformer, 
            IReusableCategoriesService reusableCategoriesService,
            ILookupTableService lookupTableService,
            ISerializer serializer)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireDocumentTransformer = questionnaireDocumentTransformer;
            this.serializer = serializer;
            this.reusableCategoriesService = reusableCategoriesService;
            this.lookupTableService = lookupTableService;
        }
        
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
        public IActionResult GetCategory(QuestionnaireRevision id, Guid categoryId)
        {
            var questionnaireId = id.OriginalQuestionnaireId ?? id.QuestionnaireId;
            var categories = this.reusableCategoriesService.GetCategoriesById(questionnaireId, categoryId);
            if (categories == null) return NotFound();

            var result = JsonConvert.SerializeObject(categories, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });

            return Content(result, "application/json", Encoding.UTF8);
        }
        
        [HttpGet]
        [Route("{id}/lookup/{lookupTableId}/headers")]
        public IActionResult GetLookupTableHeaders(QuestionnaireRevision id, Guid lookupTableId)
        {
            var lookupTableContentFile = this.lookupTableService.GetLookupTableContentFile(id, lookupTableId);
            if (lookupTableContentFile?.Content == null)
                return NotFound();

            var content = Encoding.UTF8.GetString(lookupTableContentFile.Content);
            var headerLine = content
                .Split('\n')
                .Select(line => line.Trim('\r', ' '))
                .FirstOrDefault(line => line.Length > 0);

            if (string.IsNullOrEmpty(headerLine))
                return Ok(Array.Empty<string>());

            // Lookup tables are tab-separated; fall back to comma for legacy CSV content.
            var separator = headerLine.Contains('\t') ? '\t' : ',';
            var headers = headerLine
                .Split(separator)
                .Select(header => header.Trim())
                .Where(header => header.Length > 0)
                .ToArray();

            return Ok(headers);
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
