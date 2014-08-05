using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Formatters;

namespace WB.UI.Headquarters.API.Resources
{
    [TokenValidationAuthorizationAttribute]
    [RoutePrefix("api/resources/questionnaires/v1")]
    [HeadquarterFeatureOnly]
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

            if (document == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Questionnaire with id {0} and version {1} was not found", id, version));
            }

            var response = Request.CreateResponse(HttpStatusCode.OK, document, new JsonNetFormatter(new JsonSerializerSettings
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