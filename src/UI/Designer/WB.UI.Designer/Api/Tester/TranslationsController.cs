using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [RoutePrefix("translations")]
    public class TranslationsController : ApiController
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationsController(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public TranslationDto[] Get(Guid id)
            => this.translations.Query(_ => _.Where(x => x.QuestionnaireId == id).ToList()).Cast<TranslationDto>().ToArray();
    }
}