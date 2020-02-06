using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Controllers
{
    [Route("apidocs")]
    [ApiController]
    public class ApiDocsController : ControllerBase
    {
        [Route("")]
        [Route("index")]
        [Route("index.html")]
        public IActionResult Index()
        {
            return Redirect("~/apidocs/index.html");
        }
    }
}
