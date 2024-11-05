using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection;

public abstract class GeoTrackingControllerBase : ControllerBase
{
    private readonly ILogger<GeoTrackingControllerBase> logger;

    protected GeoTrackingControllerBase(
        ILogger<GeoTrackingControllerBase> logger)
    {
        this.logger = logger;
    }

    public virtual IActionResult Post(GeoTrackingPackageApiView package)
    {
        if (package == null)
            return this.BadRequest("Server cannot accept empty package content.");

        


        return this.Ok();
    }
}
