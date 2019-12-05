using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.ReusableCategories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.API.Export
{
    [Localizable(false)]
    [RoutePrefix("api/export/v1/questionnaire")]
    public class QuestionnaireApiController : ApiController
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISerializer serializer;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire;

        public QuestionnaireApiController(IQuestionnaireStorage questionnaireStorage, ISerializer serializer, IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.pdfStorage = pdfStorage ?? throw new ArgumentNullException(nameof(pdfStorage));
            this.categoriesFillerIntoQuestionnaire = categoriesFillerIntoQuestionnaire;
        }

        [Route("{id}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            questionnaireDocument = categoriesFillerIntoQuestionnaire.FillCategoriesIntoQuestionnaireDocument(questionnaireIdentity, questionnaireDocument);

            var response = new HttpResponseMessage();
            response.Content = new StringContent(this.serializer.Serialize(questionnaireDocument), Encoding.UTF8, "application/json");
            return response;
        }

        [Route("{id}/pdf")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public HttpResponseMessage Pdf(string id, [FromUri]Guid? translation = null)
        {
            QuestionnairePdf pdf = 
                this.pdfStorage.GetById(translation.HasValue ? $"{translation.FormatGuid()}_{id}" : id);

            if (pdf == null) 
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(pdf.Content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }
    }
}
