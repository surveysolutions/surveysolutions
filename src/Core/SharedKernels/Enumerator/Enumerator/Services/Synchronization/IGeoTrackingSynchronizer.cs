using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization;

public interface IGeoTrackingSynchronizer
{
    Task SynchronizeGeoTrackingAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics, CancellationToken cancellationToken);

    void SavePackage(GeoTrackingPackageApiView package);
}

public class DummyGeoTrackingSynchronizer : IGeoTrackingSynchronizer
{
    public Task SynchronizeGeoTrackingAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void SavePackage(GeoTrackingPackageApiView package)
    {
        
    }
}
