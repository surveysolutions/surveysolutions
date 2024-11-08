using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Views;

namespace WB.UI.Shared.Extensions.Synchronization;

public class GeoTrackingSynchronizer : IGeoTrackingSynchronizer
{
    private readonly IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage;
    private readonly IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage;
    private readonly ISynchronizationService synchronizationService;
    private readonly IAssignmentDocumentsStorage assignmentsRepository;

    public GeoTrackingSynchronizer(IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage, 
        IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage,
        ISynchronizationService synchronizationService,
        IAssignmentDocumentsStorage assignmentsRepository)
    {
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.synchronizationService = synchronizationService;
        this.assignmentsRepository = assignmentsRepository;
    }
    
    public async Task SynchronizeGeoTrackingAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics,
        CancellationToken cancellationToken)
    {
        progress.Report(new SyncProgressInfo
        {
            Title = EnumeratorUIResources.Synchronization_UploadGeoTracking,
            Stage = SyncStage.UploadingGeoTracking
        });

        var assignmentIds = assignmentsRepository.LoadAll().Select(a => a.Id).ToHashSet();

        var geoTrackingRecordsToUpload = geoTrackingRecordsStorage
            .Where(r => r.IsSynchronized == false)
            .OrderBy(r => r.Id)
            .ToArray();

        var records = new List<GeoTrackingRecordApiView>();

        foreach (var geoTrackingRecord in geoTrackingRecordsToUpload)
        {
            var points = geoTrackingPointsStorage
                .Where(p => p.GeoTrackingRecordId == geoTrackingRecord.Id.Value)
                .OrderBy(p => p.Id);
            
            var apiView = new GeoTrackingRecordApiView()
            {
                InterviewerId = geoTrackingRecord.InterviewerId,
                AssignmentId = geoTrackingRecord.AssignmentId,
                Start = geoTrackingRecord.Start,
                End = geoTrackingRecord.End,
                Points = points.Select(p => new GeoTrackingPointApiView()
                {
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Time = p.Time,
                }).ToArray()
            };
            records.Add(apiView);
        }
        
        GeoTrackingPackageApiView package = new GeoTrackingPackageApiView()
        {
            Records = records.ToArray()
        };
        
        await this.synchronizationService.UploadGeoTrackingAsync(package, cancellationToken);
        
        geoTrackingRecordsToUpload.ForEach(r => r.IsSynchronized = true);
        geoTrackingRecordsStorage.Store(geoTrackingRecordsToUpload);

        var geoTrackingRecordsToRemove = geoTrackingRecordsStorage
            .Where(r => !assignmentIds.Contains(r.AssignmentId));

        // clear geo tracking records without assignments
        foreach (var record in geoTrackingRecordsToRemove)
        {
            var points = geoTrackingPointsStorage
                .Where(p => p.GeoTrackingRecordId == record.Id.Value);
            geoTrackingPointsStorage.Remove(points);
        }
        geoTrackingRecordsStorage.Remove(geoTrackingRecordsToRemove);
    }

    public void SavePackage(GeoTrackingPackageApiView package)
    {
        foreach (var record in package.Records)
        {
            var geoTrackingRecord = new GeoTrackingRecord()
            {
                InterviewerId = record.InterviewerId,
                AssignmentId = record.AssignmentId,
                Start = record.Start,
                End = record.End,
            };
            geoTrackingRecordsStorage.Store(geoTrackingRecord);

            var points = record.Points.Select(p =>
                new GeoTrackingPoint()
                {
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Time = p.Time,
                    GeoTrackingRecordId = geoTrackingRecord.Id.Value,
                });
            geoTrackingPointsStorage.Store(points);
        }
    }
}
