using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/translations")]
    [Authorize]
    public class HQTranslationsController : ControllerBase
    {
        private readonly DesignerDbContext translations;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public HQTranslationsController(DesignerDbContext translations,
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.translations = translations;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public IActionResult Get(string id)
        {
            Guid questionnaireId = Guid.Parse(id);
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
                return NotFound();
            var translationsIds = questionnaireView.Source.Translations.Select(x => x.Id).ToList();

            //Cast<TranslationDto> preserves TranslationInstance type that used during serialization
            //consumer has no idea about that type
            var translationInstances = this.translations.TranslationInstances
                .Where(x => x.QuestionnaireId == questionnaireId && translationsIds.Contains(x.TranslationId)).ToList()
                .Select(x => new TranslationDto()
                {
                    Value = x.Value,
                    Type = x.Type,
                    TranslationId = x.TranslationId,
                    QuestionnaireEntityId = x.QuestionnaireEntityId,
                    TranslationIndex = x.TranslationIndex
                }).ToList();

            return Ok(translationInstances);
        }
    }
}
