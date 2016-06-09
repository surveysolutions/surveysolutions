using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [RoutePrefix("user")]
    public class UserController : ApiController
    {
        [HttpGet]
        [Route("login")]
        public void Login(int version)
        {
            if (version < ApiVersion.Tester)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }

        [Obsolete("Since v5.10")]
        [HttpGet]
        [Route("~/api/v{version:int}/login")]
        public void OldLogin()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }
    }
}