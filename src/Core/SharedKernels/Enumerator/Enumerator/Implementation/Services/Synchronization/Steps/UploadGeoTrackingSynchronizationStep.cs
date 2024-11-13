using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;

public class UploadGeoTrackingSynchronizationStep : SynchronizationStep
{
    private readonly IGeoTrackingSynchronizer geoTrackingSynchronizer;

    public UploadGeoTrackingSynchronizationStep(int sortOrder, 
        ISynchronizationService synchronizationService, 
        ILogger logger,
        IGeoTrackingSynchronizer geoTrackingSynchronizer) 
        : base(sortOrder, synchronizationService, logger)
    {
        this.geoTrackingSynchronizer = geoTrackingSynchronizer ?? throw new ArgumentNullException(nameof(geoTrackingSynchronizer));
    }

    public override Task ExecuteAsync()
    {
        return this.geoTrackingSynchronizer.UploadGeoTrackingAsync(Context.Progress, Context.Statistics,
            Context.CancellationToken);
    }
}
