using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public class CommandService : ICommandService
    {
        private readonly IAggregateLock aggregateLock;
        private readonly IServiceLocator serviceLocator;

        private static int executingCommandsCount = 0;
        private static readonly object ExecutionCountLock = new object();
        private static TaskCompletionSource<object> executionAwaiter = null;

        public CommandService(IServiceLocator serviceLocator,
            IAggregateLock aggregateLock)
        {
            this.serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
            this.aggregateLock = aggregateLock ?? throw new ArgumentNullException(nameof(aggregateLock));
        }

        public Task ExecuteAsync(ICommand command, string origin, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Execute(command, origin, cancellationToken), cancellationToken);
        }

        public void Execute(ICommand command, string origin)
        {
            this.ExecuteImpl(command, origin, CancellationToken.None);
        }

        private void Execute(ICommand command, string origin, CancellationToken cancellationToken)
        {
            RegisterCommandExecution();

            try
            {
                this.ExecuteImpl(command, origin, cancellationToken);
            }
            finally
            {
                UnregisterCommandExecution();
            }
        }

        private static void RegisterCommandExecution()
        {
            lock (ExecutionCountLock)
            {
                executingCommandsCount++;
            }
        }

        private static void UnregisterCommandExecution()
        {
            lock (ExecutionCountLock)
            {
                executingCommandsCount--;

                if (executingCommandsCount > 0)
                    return;

                if (executionAwaiter != null)
                {
                    executionAwaiter.SetResult(new object());
                    executionAwaiter = null;
                }
            }
        }

        public Task WaitPendingCommandsAsync()
        {
            lock (ExecutionCountLock)
            {
                if (executingCommandsCount == 0)
                    return Task.FromResult(null as object);

                executionAwaiter ??= new TaskCompletionSource<object>();

                return executionAwaiter.Task;
            }
        }

        public bool HasPendingCommands => executingCommandsCount > 0;

        protected virtual void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            cancellationToken.ThrowIfCancellationRequested();

            if (!CommandRegistry.Contains(command))
                throw new CommandServiceException($"Unable to execute command {command.GetType().Name} because it is not registered.");

            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Guid aggregateId = aggregateRootIdResolver.Invoke(command);

            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                serviceLocator.ExecuteInScope<ICommandExecutor>(ce => ce.ExecuteCommand(command, origin, cancellationToken, aggregateId));
            });
        }
    }
}
