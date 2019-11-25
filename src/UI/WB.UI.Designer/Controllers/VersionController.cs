using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Designer.Controllers
{
    public class VersionController : Controller
    {
        private readonly IProductVersion productVersion;

        public VersionController(IProductVersion productVersion)
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
