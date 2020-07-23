using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ITenantContext tenantContext;

        public HealthCheckController(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        [Route(".connectivity")]
        public async Task<ActionResult> CommunicationCheck()
        {
            try
            {
                await this.tenantContext.Api.GetInterviewEvents(0, 1);
                return Ok();
            }
            catch
            {
                var apiType = this.tenantContext.Api.GetType();
                var httpClientProperty = apiType.GetProperty("Client");
                if (httpClientProperty != null)
                {
                    if (httpClientProperty.GetValue(this.tenantContext.Api) is HttpClient httpClient)
                    {
                        return BadRequest("Cannot connect back to Headquarters. Url: " + httpClient.BaseAddress);
                    }
                }
                return BadRequest("Cannot connect back to Headquarters. Url: " + this.tenantContext.Tenant.BaseUrl);
            }
        }
    }
}
