using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Snapshots;

namespace WB.Core.Infrastructure.CommandBus
{
    // TODO: TLK, KP-4337: make internal
    public class CommandService : ICommandService
    {
        private readonly ICommandService ncqrsCommandService;
        private readonly IAggregateRootRepository repository;
        private readonly IEventPublisher eventPublisher;
        private readonly ISnapshotManager snapshotManager;

        public CommandService(ICommandService ncqrsCommandService, IAggregateRootRepository repository, IEventPublisher eventPublisher, ISnapshotManager snapshotManager)
        {
            this.ncqrsCommandService = ncqrsCommandService;
            this.repository = repository;
            this.eventPublisher = eventPublisher;
            this.snapshotManager = snapshotManager;
        }

        public void Execute(ICommand command, string origin)
        {
            if (command == null) throw new ArgumentNullException("command");

            if (CommandRegistry.Contains(command))
            {
                this.ExecuteImpl(command, origin);
            }
            else
            {
                this.ncqrsCommandService.Execute(command, origin);
            }
        }

        private void ExecuteImpl(ICommand command, string origin)
        {
            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);

            Guid aggregateId = aggregateRootIdResolver.Invoke(command);

            IAggregateRoot aggregate = this.repository.GetLatest(aggregateType, aggregateId);

            if (aggregate == null)
            {
                if (!CommandRegistry.IsConstructor(command))
                    throw new Exception(string.Format("Unable to execute not-constructing command {0} because aggregate {1} does not exist.", command.GetType().Name, aggregateId.FormatGuid()));

                aggregate = (IAggregateRoot) Activator.CreateInstance(aggregateType);
                aggregate.SetId(aggregateId);
            }

            commandHandler.Invoke(command, aggregate);

            this.eventPublisher.PublishUncommitedEventsFromAggregateRoot(aggregate, origin);

            this.snapshotManager.CreateSnapshotIfNeededAndPossible(aggregate);
        }
    }
}