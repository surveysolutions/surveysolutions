using System;
using System.Collections.Generic;
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
    [ApiBasicAuth(onlyAllowedAddresses: true)]
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
            //Cast<TranslationDto> preserves TranslationInstance type that used during serialization
            //consumer has no idea about that type
            var translationInstances = this.translations.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId).ToList())
                .Select(x => new TranslationDto()
                {
                    Value = x.Value,
                    Type = x.Type,
                    TranslationId = x.TranslationId,
                    QuestionnaireEntityId = x.QuestionnaireEntityId,
                    TranslationIndex = x.TranslationIndex
                }).ToList();

            return translationInstances.Count == 0
                ? this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"No translations found for questionnaire Id: {id}")
                : this.Request.CreateResponse(HttpStatusCode.OK, translationInstances);
        }
    }
}