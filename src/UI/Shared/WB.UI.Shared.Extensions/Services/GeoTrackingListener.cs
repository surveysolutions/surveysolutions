using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.Views;

namespace WB.UI.Shared.Extensions.Services;

public interface IGeoTrackingListener : IGeolocationListener
{
    void Init(int assignmentId);
    void Stop();
}

public class GeoTrackingListener : IGeoTrackingListener
{
    private readonly IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage;
    private readonly IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage;
    private readonly IPrincipal principal;

    private int? lastRecordId;
    private int? assignmentId;

    public GeoTrackingListener(IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage, 
        IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage,
        IPrincipal principal)
    {
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.principal = principal;
    }

    public void Init(int assignmentId)
    {
        this.assignmentId = assignmentId;
        this.lastRecordId = null;
    }

    private void StartNewTrack(int assignmentId)
    {
        var record = new GeoTrackingRecord()
        {
            InterviewerId = principal.CurrentUserIdentity.UserId,
            AssignmentId = assignmentId,
            Start = DateTimeOffset.Now
        };
        geoTrackingRecordsStorage.Store(record);
        
        lastRecordId = record.Id;
    }

    public void Stop()
    {
        if (!lastRecordId.HasValue)
            return;
        
        var geoTrackingRecord = geoTrackingRecordsStorage.GetById(lastRecordId.Value);
        if (geoTrackingRecord.End == null)
        {
            geoTrackingRecord.End = DateTimeOffset.Now;
            geoTrackingRecordsStorage.Store(geoTrackingRecord);
        }
    }

    public Task OnGpsLocationChanged(GpsLocation location, INotificationManager notifications)
    {
        if (!lastRecordId.HasValue)
        {
            if (!assignmentId.HasValue)
                return Task.CompletedTask;
            
            StartNewTrack(assignmentId.Value);
        }
        
        var point = new GeoTrackingPoint()
        {
            GeoTrackingRecordId = lastRecordId.Value,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Time = location.Timestamp,
        };
        geoTrackingPointsStorage.Store(point);

        return Task.CompletedTask;
    }
}
