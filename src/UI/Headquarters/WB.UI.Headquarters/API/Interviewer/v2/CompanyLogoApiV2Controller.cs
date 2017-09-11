using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class CompanyLogoApiV2Controller : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> logoStorage;

        public CompanyLogoApiV2Controller(IPlainKeyValueStorage<CompanyLogo> logoStorage)
        {
            this.logoStorage = logoStorage;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            var incomingEtag = Request.Headers.IfNoneMatch.FirstOrDefault()?.Tag ?? "";
            var companyLogo = this.logoStorage.GetById(CompanyLogo.CompanyLogoStorageKey);

            if (companyLogo == null) return Request.CreateResponse(HttpStatusCode.NoContent);

            var etagValue = companyLogo.GetEtagValue();
            if (etagValue.Equals(incomingEtag.Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return Request.CreateResponse(HttpStatusCode.NotModified);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(companyLogo.Logo);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
            response.Headers.ETag = new EntityTagHeaderValue($"\"{etagValue}\"");

            return response;
        }
    }
}