using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandService
    {
        void Execute(ICommand command, string origin = null, bool handleInBatch = false);
        Task ExecuteAsync(ICommand command, string origin = null, CancellationToken cancellationToken = default(CancellationToken));
        Task WaitPendingCommandsAsync();
    }
}
