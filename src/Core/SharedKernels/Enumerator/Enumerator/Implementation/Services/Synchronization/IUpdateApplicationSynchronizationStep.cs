using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public interface IUpdateApplicationSynchronizationStep : ISynchronizationStep
    {
        Task CheckServerVersionAsync(CancellationToken cancellationToken);
    }
}
