using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Domain.Storage;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.Implementation.CommandBus
{
    internal class CommandService : ICommandService
    {
        private readonly IAggregateRootRepository repository;
        private readonly ILiteEventBus eventBus;
        private readonly IAggregateSnapshotter snapshooter;

        private int executingCommandsCount = 0;
        private readonly object executionCountLock = new object();
        private TaskCompletionSource<object> executionAwaiter = null;

        public CommandService(IAggregateRootRepository repository, ILiteEventBus eventBus, IAggregateSnapshotter snapshooter)
        {
            this.repository = repository;
            this.eventBus = eventBus;
            this.snapshooter = snapshooter;
        }

        public Task ExecuteAsync(ICommand command, string origin, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Execute(command, origin, cancellationToken));
        }

        public void Execute(ICommand command, string origin)
        {
            this.Execute(command, origin, CancellationToken.None);
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
            lock (this.executionCountLock)
            {
                this.executingCommandsCount++;
            }
        }

        private void UnregisterCommandExecution()
        {
            lock (this.executionCountLock)
            {
                this.executingCommandsCount--;

                if (this.executingCommandsCount > 0)
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
            lock (this.executionCountLock)
            {
                if (this.executingCommandsCount == 0)
                    return Task.FromResult(null as object);

                if (this.executionAwaiter == null)
                {
                    this.executionAwaiter = new TaskCompletionSource<object>();
                }

                return this.executionAwaiter.Task;
            }
        }

        protected virtual void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException("command");

            cancellationToken.ThrowIfCancellationRequested();

            if (!CommandRegistry.Contains(command))
                throw new CommandServiceException(string.Format("Unable to execute command {0} because it is not registered.", command.GetType().Name));

            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);
            Func<IAggregateRoot> constructor = CommandRegistry.GetAggregateRootConstructor(command);

            Guid aggregateId = aggregateRootIdResolver.Invoke(command);

            IAggregateRoot aggregate = this.repository.GetLatest(aggregateType, aggregateId);

            cancellationToken.ThrowIfCancellationRequested();

            if (aggregate == null)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException(string.Format("Unable to execute not-constructing command {0} because aggregate {1} does not exist.", command.GetType().Name, aggregateId.FormatGuid()));

                aggregate = constructor.Invoke();
                aggregate.SetId(aggregateId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            commandHandler.Invoke(command, aggregate);

            this.eventBus.PublishUncommitedEventsFromAggregateRoot(aggregate, origin);

            this.snapshooter.CreateSnapshotIfNeededAndPossible(aggregate);
        }
    }
}