using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.API.Export
{
    [RoutePrefix("api/export/v1")]
    public class QuestionnaireApiController : ApiController
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISerializer serializer;

        public QuestionnaireApiController(IQuestionnaireStorage questionnaireStorage, ISerializer serializer)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        [Route("questionnaire/{id}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public JsonResult<QuestionnaireDocument> Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            QuestionnaireDocument questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            return Json(questionnaireDocumentVersioned, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
