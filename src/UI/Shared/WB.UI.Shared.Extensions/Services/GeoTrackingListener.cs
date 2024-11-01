using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.Views;

namespace WB.UI.Shared.Extensions.Services;

public interface IGeoTrackingListener : IGeolocationListener
{
    void Start(int assignmentId);
    void Stop();
}

public class GeoTrackingListener : IGeoTrackingListener
{
    private readonly IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage;
    private readonly IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage;

    private int? lastRecordId;

    public GeoTrackingListener(IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage, 
        IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage)
    {
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
    }

    public void Start(int assignmentId)
    {
        var record = new GeoTrackingRecord()
        {
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
        if (lastRecordId.HasValue)
        {
            var point = new GeoTrackingPoint()
            {
                GeoTrackingRecordId = lastRecordId.Value,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Time = location.Timestamp,
            };
            geoTrackingPointsStorage.Store(point);
        }

        return Task.CompletedTask;
    }
    
    public GeolocationListenerResult LastResult { get; set; }
}
