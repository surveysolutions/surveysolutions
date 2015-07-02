using System;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.Implementation.CommandBus
{
    internal class CommandService : ICommandService
    {
        private readonly IAggregateRootRepository repository;
        private readonly IEventBus eventBus;
        private readonly IAggregateSnapshotter snapshooter;

        public CommandService(IAggregateRootRepository repository, IEventBus eventBus, IAggregateSnapshotter snapshooter)
        {
            this.repository = repository;
            this.eventBus = eventBus;
            this.snapshooter = snapshooter;
        }

        public void Execute(ICommand command, string origin, bool handleInBatch)
        {
            if (command == null) throw new ArgumentNullException("command");

            if (!CommandRegistry.Contains(command))
                throw new CommandServiceException(string.Format("Unable to execute command {0} because it is not registered.", command.GetType().Name));

            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);
            Func<IAggregateRoot> constructor = CommandRegistry.GetAggregateRootConstructor(command);

            Guid aggregateId = aggregateRootIdResolver.Invoke(command);

            IAggregateRoot aggregate = this.repository.GetLatest(aggregateType, aggregateId);

            if (aggregate == null)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException(string.Format("Unable to execute not-constructing command {0} because aggregate {1} does not exist.", command.GetType().Name, aggregateId.FormatGuid()));

                aggregate = constructor.Invoke();
                aggregate.SetId(aggregateId);
            }

            commandHandler.Invoke(command, aggregate);

            this.eventBus.PublishUncommitedEventsFromAggregateRoot(aggregate, origin, handleInBatch);

            this.snapshooter.CreateSnapshotIfNeededAndPossible(aggregate);
        }
    }
}