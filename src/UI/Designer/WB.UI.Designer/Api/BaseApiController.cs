using System.Web.Http;

namespace WB.UI.Designer.Api
{
    public class BaseApiController : ApiController
    {
        [Route("~/api/v1/login")]
        [HttpGet]
        public void Login() { }
    }
}