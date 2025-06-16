using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.GeoTracking;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection;

public abstract class GeoTrackingControllerBase : ControllerBase
{
    private readonly ILogger<GeoTrackingControllerBase> logger;
    private readonly IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage;
    private readonly IAssignmentsService assignmentsService;

    protected GeoTrackingControllerBase(
        ILogger<GeoTrackingControllerBase> logger,
        IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage,
        IAssignmentsService assignmentsService)
    {
        this.logger = logger;
        this.geoTrackingStorage = geoTrackingStorage;
        this.assignmentsService = assignmentsService;
    }

    public virtual IActionResult Post(GeoTrackingPackageApiView package)
    {
        if (package == null)
            return this.BadRequest("Server cannot accept empty package content.");

        var assignmentsIds = package.Records.Select(r => r.AssignmentId);
        var uniqueAssignmentIds = new HashSet<int>(assignmentsIds);
        var existedAssignmentIds = assignmentsService.GetExistingAssignmentIds(uniqueAssignmentIds);
        
        foreach (var record in package.Records)
        {
            if (!existedAssignmentIds.Contains(record.AssignmentId))
            {
                this.logger.LogWarning("Assignment {AssignmentId} for GeoTracking record is not found.", record.AssignmentId);
                continue;
            }
            
            var points = record.Points.Select(p => new GeoTrackingPoint()
            {
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Time = p.Time.UtcDateTime,
            }).ToList();
            var geoTrackingRecord = new GeoTrackingRecord()
            {
                AssignmentId = record.AssignmentId,
                InterviewerId = record.InterviewerId,
                Start = record.Start.UtcDateTime,
                End = record.End?.UtcDateTime,
                Points = points
            };
            
            geoTrackingStorage.Store(geoTrackingRecord, null);
        }

        return this.Ok();
    }
}
