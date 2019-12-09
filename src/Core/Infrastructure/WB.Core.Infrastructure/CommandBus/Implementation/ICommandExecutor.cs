using System;
using System.Threading;

namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public interface ICommandExecutor
    {
        void ExecuteCommand(ICommand command, string origin, CancellationToken cancellationToken, Guid aggregateId);
    }
}
