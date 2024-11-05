using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;

[Authorize(Roles = "Interviewer")]
public class GeoTrackingApiV2Controller : GeoTrackingControllerBase
{
    public GeoTrackingApiV2Controller(ILogger<GeoTrackingControllerBase> logger) : base(logger)
    {
    }

    [HttpPost]
    [Route("api/interviewer/v2/geotracking")]
    public override IActionResult Post([FromBody]GeoTrackingPackageApiView package) => base.Post(package);
}
