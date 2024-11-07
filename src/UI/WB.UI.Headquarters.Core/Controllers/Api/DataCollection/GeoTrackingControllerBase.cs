using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.GeoTracking;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection;

public abstract class GeoTrackingControllerBase : ControllerBase
{
    private readonly ILogger<GeoTrackingControllerBase> logger;
    private readonly IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage;

    protected GeoTrackingControllerBase(
        ILogger<GeoTrackingControllerBase> logger,
        IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage)
    {
        this.logger = logger;
        this.geoTrackingStorage = geoTrackingStorage;
    }

    public virtual IActionResult Post(GeoTrackingPackageApiView package)
    {
        if (package == null)
            return this.BadRequest("Server cannot accept empty package content.");

        foreach (var record in package.Records)
        {
            var points = record.Points.Select(p => new GeoTrackingPoint()
            {
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Time = p.Time,
            }).ToList();
            var geoTrackingRecord = new GeoTrackingRecord()
            {
                AssignmentId = record.AssignmentId,
                InterviewerId = record.InterviewerId,
                Start = record.Start,
                End = record.End,
                Points = points
            };
            geoTrackingStorage.Store(geoTrackingRecord, null);
        }

        return this.Ok();
    }
}
