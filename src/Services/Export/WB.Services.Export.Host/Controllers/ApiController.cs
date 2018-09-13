using System;
using System.Net.Http;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc;

namespace WB.Services.Export.Host.Controllers
{
    [Route("api/v1/{tenant}")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient backgroundJobClient;

        public JobController(IBackgroundJobClient backgroundJobClient)
        {
            this.backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        }

        [HttpGet]
        public string Get()
        {
            return "Hello world, " + RouteData.Values["tenant"];
        }

        [HttpPost]
        public ActionResult Post()
        {
            return Ok();
        }
    }
}
