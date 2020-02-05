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
        private readonly IWebHostEnvironment hostEnvironment;

        public ApiDocsController(IWebHostEnvironment  hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
        }

        [Route("")]
        [Route("index")]
        [Route("index.html")]
        public IActionResult Index()
        {
            var pathToHtml = Path.Combine(hostEnvironment.WebRootPath, "apidocs", "index.html");
            var fileStream = System.IO.File.OpenRead(pathToHtml);
            return File(fileStream, "text/html");
        }
    }
}
