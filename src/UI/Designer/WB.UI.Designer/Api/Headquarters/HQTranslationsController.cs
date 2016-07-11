using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Headquarters
{
    [ApiBasicAuth]
    [RoutePrefix("api/hq/translations")]
    public class HQTranslationsController : ApiController
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public HQTranslationsController(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public HttpResponseMessage Get(string id)
        {
            Guid questionnaireId = Guid.Parse(id);
            var translationInstances = 
                this.translations.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId).ToList());

            if (translationInstances.Count == 0)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"No translations found questionnaireId: {questionnaireId}, culture: {id}");
            }

            var dtos = translationInstances.Select(x => new TranslationDto
            {
                Language = x.Language,
                Value = x.Translation,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                Type = x.Type,
                TranslationIndex = x.TranslationIndex
            }).ToList();

            var result = Request.CreateResponse(HttpStatusCode.OK, dtos);
            return result;
        }
    }
}