using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class TranslationsControllerBase : ApiController
    {
        private readonly ITranslationManagementService translations;

        protected TranslationsControllerBase(ITranslationManagementService translations)
        {
            this.translations = translations;
        }

        public virtual HttpResponseMessage Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            var translationInstances = this.translations.GetAll(questionnaireIdentity).Cast<TranslationDto>();

            return Request.CreateResponse(HttpStatusCode.OK, translationInstances);
        }
    }
}
