using System.Net.Http;
using System.Web.Http;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.WebTester.Controllers
{
    [RoutePrefix("api")]
    public class DiagnosticsController : ApiController
    {
        private readonly IProductVersion productVersion;

        public DiagnosticsController(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        [HttpGet]
        [Route("version")]
        public HttpResponseMessage Version()
        {
            var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(productVersion.ToString());

            return response;
        }
    }
}
