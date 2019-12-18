using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class TranslationsControllerBase : ControllerBase
    {
        private readonly ITranslationManagementService translations;

        protected TranslationsControllerBase(ITranslationManagementService translations)
        {
            this.translations = translations;
        }

        public virtual ActionResult<List<TranslationDto>> Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            var translationInstances = this.translations.GetAll(questionnaireIdentity).Cast<TranslationDto>().ToList();

            return translationInstances;
        }
    }
}
