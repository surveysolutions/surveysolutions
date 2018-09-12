using Microsoft.AspNetCore.Mvc;

namespace WB.Services.Export.Host.Controllers
{
    [Route("api/v1/{tenant}")]
    [ApiController]
    public class JobController : ControllerBase
    {
        public string Get()
        {
            return "Hello world, " + RouteData.Values["tenant"];
        }
    }
}
