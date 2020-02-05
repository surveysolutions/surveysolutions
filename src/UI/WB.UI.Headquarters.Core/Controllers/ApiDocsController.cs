using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public ContentResult Index()
        {
            var pathToHtml = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apidocs", "index.html");
            var content = System.IO.File.ReadAllText(pathToHtml);
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = content
            };
        }
    }
}
