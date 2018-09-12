using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace WB.Services.Export.Host.Controllers
{
    [Route(".version")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        public string Get()
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductVersion;
        }
    }
}
