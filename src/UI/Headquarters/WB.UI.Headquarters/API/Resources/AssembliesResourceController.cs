using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Resources
{
    [TokenValidationAuthorization]
    [RoutePrefix("api/resources/assemblies/v1")]
    [HeadquarterFeatureOnly]
    public class AssembliesResourceController : ApiController
    {
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public AssembliesResourceController(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        [Route("{id}/{version:int}", Name = "api.questionnaireAssembly")]
        public HttpResponseMessage Get(string id, long version)
        {
            byte[] assembly = this.questionnareAssemblyFileAccessor.GetAssemblyAsByteArray(Guid.Parse(id), version);

            if (assembly == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Questionnaire assembly with id {0} and version {1} was not found", id, version));
            }

            HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new ByteArrayContent(assembly);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromDays(100),
                Public = true
            };

            return response;
        }
    }
}