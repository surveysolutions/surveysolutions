using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1;

[Authorize(Roles = "Supervisor")]
public class GeoTrackingApiV1Controller : GeoTrackingControllerBase
{
    public GeoTrackingApiV1Controller(ILogger<GeoTrackingControllerBase> logger) : base(logger)
    {
    }

    [HttpPost]
    [Route("api/supervisor/v1/geotracking")]
    public override IActionResult Post([FromBody]GeoTrackingPackageApiView package) => base.Post(package);
}
