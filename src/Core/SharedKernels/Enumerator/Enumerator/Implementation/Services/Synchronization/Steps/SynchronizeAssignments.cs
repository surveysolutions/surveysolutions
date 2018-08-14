using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class SynchronizeAssignments : SynchronizationStep
    {
        private readonly IAssignmentsSynchronizer assignmentsSynchronizer;

        public SynchronizeAssignments(IAssignmentsSynchronizer assignmentsSynchronizer, ISynchronizationService synchronizationService, ILogger logger, 
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.assignmentsSynchronizer = assignmentsSynchronizer;
        }

        public override Task ExecuteAsync()
        {
            return this.assignmentsSynchronizer.SynchronizeAssignmentsAsync(Context.Progress, Context.Statistics,
                Context.CancellationToken);
        }
    }
}
