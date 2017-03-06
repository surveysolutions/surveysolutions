using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class CompanyLogoApiV2Controller : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> logoStorage;

        public CompanyLogoApiV2Controller(IPlainKeyValueStorage<CompanyLogo> logoStorage)
        {
            this.logoStorage = logoStorage;
        }

        public HttpResponseMessage Get()
        {
            var incomingEtag = HttpContext.Current.Request.Headers[@"If-None-Match"];
            var companyLogo = this.logoStorage.GetById(CompanyLogo.StorageKey);

            if (companyLogo == null) return Request.CreateResponse(HttpStatusCode.NoContent);

            var etagValue = companyLogo.GetEtagValue();
            if (etagValue.Equals(incomingEtag, StringComparison.InvariantCultureIgnoreCase))
            {
                return Request.CreateResponse(HttpStatusCode.NotModified);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(companyLogo.Logo);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
            response.Headers.ETag = new EntityTagHeaderValue(etagValue);
            return response;
        }
    }
}