using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.UI.Headquarters.API.Formatters;

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

            var response = Request.CreateResponse((HttpStatusCode)200, document, new JsonNetFormatter(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, 
                NullValueHandling = NullValueHandling.Ignore
            }));

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(100),
                Public = true
            };

            return response;
        }
    }
}