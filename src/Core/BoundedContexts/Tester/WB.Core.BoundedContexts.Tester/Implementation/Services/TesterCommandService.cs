using System.Threading;
using Ncqrs.Domain.Storage;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterCommandService : SequentialCommandService
    {
        private readonly IExecutedCommandsStorage executedCommandsStorage;

        public TesterCommandService(IEventSourcedAggregateRootRepository eventSourcedRepository,
            ILiteEventBus eventBus,
            IAggregateSnapshotter snapshooter,
            IServiceLocator serviceLocator,
            IPlainAggregateRootRepository plainRepository,
            IAggregateLock aggregateLock,
            IAggregateRootCacheCleaner aggregateRootCacheCleaner,
            IExecutedCommandsStorage executedCommandsStorage
            ) : 
                base(eventSourcedRepository, eventBus, snapshooter,
                     serviceLocator, plainRepository, aggregateLock, aggregateRootCacheCleaner)
        {
            this.executedCommandsStorage = executedCommandsStorage;
        }

        protected override void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            base.ExecuteImpl(command, origin, cancellationToken);

            if (command is InterviewCommand interviewCommand)
            {
                this.executedCommandsStorage.Add(interviewCommand.InterviewId, interviewCommand);
            }
        }
    }
}
