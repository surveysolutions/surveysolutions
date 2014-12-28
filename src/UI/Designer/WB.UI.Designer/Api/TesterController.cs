using System.Net.Http;
using System.Net;
using System.Web.Http;
using Resources;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class TesterController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAllTemplates()
        {
            return Request.CreateErrorResponse(HttpStatusCode.UpgradeRequired, Strings.ApplicationUpdateRequired);
        }

        [HttpPost]
        public HttpResponseMessage ValidateCredentials()
        {
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}
