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
        private static readonly object executionCountLock = new object();
        private TaskCompletionSource<object> executionAwaiter = null;
        private TaskCompletionSource<object> commandAwaiter = null;

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
            this.RegisterCommandExecution();

            try
            {
                this.ExecuteImpl(command, origin, cancellationToken);
            }
            finally
            {
                this.UnregisterCommandExecution();
            }
        }

        private void RegisterCommandExecution()
        {
            lock (executionCountLock)
            {
                executingCommandsCount++;

                if (this.commandAwaiter != null)
                {
                    this.commandAwaiter.SetResult(new object());
                    this.commandAwaiter = null;
                }
            }
        }

        private void UnregisterCommandExecution()
        {
            lock (executionCountLock)
            {
                executingCommandsCount--;

                if (executingCommandsCount > 0)
                    return;

                if (this.executionAwaiter != null)
                {
                    this.executionAwaiter.SetResult(new object());
                    this.executionAwaiter = null;
                }
            }
        }

        public Task WaitPendingCommandsAsync()
        {
            lock (executionCountLock)
            {
                if (executingCommandsCount == 0)
                    return Task.FromResult(null as object);

                if (this.executionAwaiter == null)
                {
                    this.executionAwaiter = new TaskCompletionSource<object>();
                }

                return this.executionAwaiter.Task;
            }
        }

        public Task WaitOnCommandAsync()
        {
            lock (executionCountLock)
            {
                if (executingCommandsCount > 0)
                    return Task.FromResult(null as object);
                
                if (this.commandAwaiter == null)
                {
                    this.commandAwaiter = new TaskCompletionSource<object>();
                }

                return this.commandAwaiter.Task;
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
