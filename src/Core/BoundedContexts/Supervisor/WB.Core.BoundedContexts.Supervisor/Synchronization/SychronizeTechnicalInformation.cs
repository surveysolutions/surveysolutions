using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SychronizeTechnicalInformation : SynchronizationStep
    {
        private readonly ITechInfoSynchronizer techInfoSynchronizer;

        public SychronizeTechnicalInformation(int sortOrder, ISynchronizationService synchronizationService, ITechInfoSynchronizer techInfoSynchronizer, ILogger logger) : base(sortOrder, synchronizationService, logger)
        {
            this.techInfoSynchronizer = techInfoSynchronizer;
        }

        public override Task ExecuteAsync()
        {
            return this.techInfoSynchronizer.SynchronizeAsync(Context.Progress, Context.Statistics,
                Context.CancellationToken);
        }
    }
}
