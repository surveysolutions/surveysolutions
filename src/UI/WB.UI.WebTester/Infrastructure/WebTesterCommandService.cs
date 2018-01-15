using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterCommandService : ICommandService
    {
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly IAppdomainsPerInterviewManager interviews;
        private readonly ILiteEventBus eventBus;
        private readonly IEventStore eventStore;
        private readonly IAggregateLock aggregateLock;
        private readonly IServiceLocator serviceLocator;

        public WebTesterCommandService(
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            IAppdomainsPerInterviewManager interviews,
            ILiteEventBus eventBus,
            IEventStore eventStore,
            IAggregateLock aggregateLock,
            IServiceLocator serviceLocator)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.interviews = interviews;
            this.eventBus = eventBus;
            this.eventStore = eventStore;
            this.aggregateLock = aggregateLock;
            this.serviceLocator = serviceLocator;
        }

        public void Execute(ICommand command, string origin = null)
        {
            var aggregateId = CommandRegistry.GetAggregateRootIdResolver(command).Invoke(command);
            var aggregateType = CommandRegistry.GetAggregateRootType(command);

            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregate = this.eventSourcedRepository.GetLatest(aggregateType, aggregateId);

                if (aggregate == null)
                {
                    if (!CommandRegistry.IsInitializer(command))
                        throw new CommandServiceException($"Unable to execute not-constructing command {command.GetType().Name} because aggregate {aggregateId.FormatGuid()} does not exist.");

                    aggregate = (IEventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateType);
                    aggregate.SetId(aggregateId);
                }

                var events = this.interviews.Execute(command);
                
                aggregate.InitializeFromHistory(aggregateId, events);

                var uncommitedStream = new UncommittedEventStream(origin, events.Select(ce =>
                    new UncommittedEvent(
                        ce.EventIdentifier,
                        ce.EventSourceId,
                        ce.EventSequence,
                        aggregate.InitialVersion,
                        ce.EventTimeStamp,
                        ce.Payload
                    )));

                this.eventStore.Store(uncommitedStream);

                eventBus.PublishCommittedEvents(events);
            });
        }

        public Task ExecuteAsync(ICommand command, string origin = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task WaitPendingCommandsAsync() => Task.CompletedTask;

        public bool HasPendingCommands => false;
    }
}