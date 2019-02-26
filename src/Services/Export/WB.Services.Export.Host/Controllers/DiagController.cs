using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WB.Services.Export.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class DiagController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IProductVersion productVersion;

        public DiagController(IConfiguration configuration, IProductVersion productVersion)
        {
            this.configuration = configuration;
            this.productVersion = productVersion;
        }

        [Route(".diag")]
        public ActionResult Diagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Export service version: " + productVersion.ToString());
            foreach (var config in this.configuration.AsEnumerable())
            {
                sb.AppendLine($"[{config.Key}]={config.Value}");
            }

            return Content(sb.ToString());
        }
    }
}
