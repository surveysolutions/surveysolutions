using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterCommandService : ICommandService
    {
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly IAggregateRootCache aggregateRootCache;
        private readonly IAppdomainsPerInterviewManager interviews;
        private readonly ILiteEventBus eventBus;
        private readonly IAggregateLock aggregateLock;
        private readonly IServiceLocator serviceLocator;
        private readonly ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage;

        public WebTesterCommandService(
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            IAggregateRootCache aggregateRootCache,
            IAppdomainsPerInterviewManager interviews,
            ILiteEventBus eventBus,
            IAggregateLock aggregateLock,
            IServiceLocator serviceLocator,
            ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.aggregateRootCache = aggregateRootCache;
            this.interviews = interviews;
            this.eventBus = eventBus;
            this.aggregateLock = aggregateLock;
            this.serviceLocator = serviceLocator;
            this.executedCommandsStorage = executedCommandsStorage;
        }

        public void Execute(ICommand command, string? origin = null)
        {
            var interviewCommand = command as InterviewCommand;

            if (interviewCommand == null)
                throw new ArgumentException(nameof(command));

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

                    this.aggregateRootCache.SetAggregateRoot(aggregate);
                }

                var events = this.interviews.Execute(command);
                
                aggregate.InitializeFromHistory(aggregateId, events);

                eventBus.PublishCommittedEvents(events);

                var commands = this.executedCommandsStorage.Get(interviewCommand.InterviewId, interviewCommand.InterviewId) ?? new List<InterviewCommand>();
                commands.Add(interviewCommand);
                this.executedCommandsStorage.Store(commands, interviewCommand.InterviewId, interviewCommand.InterviewId);
            });
        }

        public Task ExecuteAsync(ICommand command, string? origin = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task WaitPendingCommandsAsync() => Task.CompletedTask;
        public Task WaitOnCommandAsync() => Task.CompletedTask;

        public bool HasPendingCommands => false;
    }
}
