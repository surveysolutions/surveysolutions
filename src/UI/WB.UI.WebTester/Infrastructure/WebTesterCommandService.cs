using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterCommandService : ICommandService
    {
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly IAggregateRootCacheFiller cacheFiller;
        private readonly IAppdomainsPerInterviewManager interviews;
        private readonly ILiteEventBus eventBus;
        private readonly IAggregateLock aggregateLock;
        private readonly IServiceLocator serviceLocator;

        public WebTesterCommandService(
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            IAggregateRootCacheFiller cacheFiller,
            IAppdomainsPerInterviewManager interviews,
            ILiteEventBus eventBus,
            IAggregateLock aggregateLock,
            IServiceLocator serviceLocator)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.cacheFiller = cacheFiller;
            this.interviews = interviews;
            this.eventBus = eventBus;
            this.aggregateLock = aggregateLock;
            this.serviceLocator = serviceLocator;
        }

        public void Execute(ICommand command, string origin = null)
        {
            var interviewCommand = command as InterviewCommand;
            var aggregateId = interviewCommand.InterviewId;
            var aggregateType = typeof(WebTesterStatefulInterview);

            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregate = this.eventSourcedRepository.GetLatest(aggregateType, aggregateId);

                if (aggregate == null)
                {
                    if (!(command is CreateInterview))
                        throw new CommandServiceException($"Unable to execute not-constructing command {command.GetType().Name} because aggregate {aggregateId.FormatGuid()} does not exist.");

                    aggregate = (IEventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateType);
                    aggregate.SetId(aggregateId);

                    this.cacheFiller.Store(aggregate);
                }

                var events = this.interviews.Execute(command);
                
                aggregate.InitializeFromHistory(aggregateId, events);

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