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
    private readonly IAuditLogService auditLogService;

    public GeoTrackingSynchronizer(IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage, 
        IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage,
        ISynchronizationService synchronizationService,
        IAssignmentDocumentsStorage assignmentsRepository,
        IAuditLogService auditLogService)
    {
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.synchronizationService = synchronizationService;
        this.assignmentsRepository = assignmentsRepository;
        this.auditLogService = auditLogService;
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
            .Where(r => !assignmentIds.Contains(r.AssignmentId))
            .OrderBy(r => r.Id);

        var records = new List<GeoTrackingRecordApiView>();

        foreach (var geoTrackingRecord in geoTrackingRecordsToUpload)
        {
            var points = geoTrackingPointsStorage
                .Where(p => p.GeoTrackingRecordId == geoTrackingRecord.Id.Value)
                .OrderBy(p => p.Id);
            
            var apiView = new GeoTrackingRecordApiView()
            {
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
    }
}
