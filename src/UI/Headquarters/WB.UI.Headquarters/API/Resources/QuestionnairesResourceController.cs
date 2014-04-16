using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;

namespace WB.UI.Headquarters.API.Resources
{
    [RoutePrefix("api/resources/questionnaires/v1")]
    public class QuestionnairesResourceController : ApiController
    {
        private readonly IVersionedQuestionnaireReader versionedQuestionnaireReader;

        public QuestionnairesResourceController(IVersionedQuestionnaireReader versionedQuestionnaireReader)
        {
            if (versionedQuestionnaireReader == null) throw new ArgumentNullException("versionedQuestionnaireReader");
            
            this.versionedQuestionnaireReader = versionedQuestionnaireReader;
        }

        [Route("{id}/{version:int}", Name = "api.questionnaireDetails")]
        public HttpResponseMessage Get(string id, long version)
        {
            var document = this.versionedQuestionnaireReader.Get(id, version);
            var response = Request.CreateResponse(document);

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(100),
                Public = true
            };

            return response;
        }
    }
}