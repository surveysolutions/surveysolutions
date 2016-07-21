using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [RoutePrefix("translation")]
    public class TranslationController : ApiController
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationController(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public TranslationDto[] Get(Guid id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            return this.translations.Query(_ => _.Where(x => x.QuestionnaireId == id).ToList()).Select(x => new TranslationDto()
            {
                Value = x.Value,
                Type = x.Type,
                TranslationId = x.TranslationId,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                TranslationIndex = x.TranslationIndex
            }).ToArray();
        }
    }
}