using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.WebTester.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly IProductVersion productVersion;

        public DiagnosticsController(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        [HttpGet]
        [Route(".version")]
        public IActionResult Version()
        {
            return Content(productVersion.ToString());
        }
    }
}
