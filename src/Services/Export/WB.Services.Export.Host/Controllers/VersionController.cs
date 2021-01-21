using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        // GET
        [Route("/.version")]
        public IActionResult Index()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var productVersion = fvi.FileVersion;
            return Ok(productVersion);
        }
    }
}
