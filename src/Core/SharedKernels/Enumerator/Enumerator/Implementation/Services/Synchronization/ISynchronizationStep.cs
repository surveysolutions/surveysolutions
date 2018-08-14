using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public interface ISynchronizationStep
    {
        int SortOrder { get; }

        EnumeratorSynchonizationContext Context { get; set; }

        Task ExecuteAsync();
    }
}
