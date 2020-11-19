using System;
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
            string productVersion = fvi.FileVersion ?? String.Empty;
            return Ok(productVersion);
        }
    }
}
