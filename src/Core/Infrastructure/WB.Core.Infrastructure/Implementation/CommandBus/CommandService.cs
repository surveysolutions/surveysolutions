using System;
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
        private int executingCommandsCount;

        public CommandService(IAggregateRootRepository repository, ILiteEventBus eventBus, IAggregateSnapshotter snapshooter)
        {
            this.repository = repository;
            this.eventBus = eventBus;
            this.snapshooter = snapshooter;
        }

        public async Task ExecuteAsync(ICommand command, string origin, CancellationToken cancellationToken)
        {
            await Task.Run(() => this.Execute(command, origin, cancellationToken));
        }

        public void Execute(ICommand command, string origin)
        {
            this.Execute(command, origin, CancellationToken.None);
        }

        private void Execute(ICommand command, string origin, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref this.executingCommandsCount);
            try
            {
                this.ExecuteImpl(command, origin, cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref this.executingCommandsCount);
            }
        }

        public async Task WaitPendingCommandsAsync()
        {
            while (this.executingCommandsCount > 0)
            {
                await Task.Delay(100);
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