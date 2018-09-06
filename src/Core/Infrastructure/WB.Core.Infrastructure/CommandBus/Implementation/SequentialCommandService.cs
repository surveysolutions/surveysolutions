using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public class SequentialCommandService : CommandService
    {
        private class CommandDescriptor
        {
            public CommandDescriptor(ICommand command, string origin, CancellationToken cancellationToken)
            {
                this.Command = command;
                this.Origin = origin;
                this.CancellationToken = cancellationToken;
            }

            public ICommand Command { get; private set; }
            public string Origin { get; private set; }
            public CancellationToken CancellationToken { get; private set; }
        }

        private readonly ConcurrentQueue<CommandDescriptor> queue = new ConcurrentQueue<CommandDescriptor>();
        private readonly object lockObject = new object();

        public SequentialCommandService(IEventSourcedAggregateRootRepository eventSourcedRepository, 
            ILiteEventBus eventBus, 
            IAggregateSnapshotter snapshooter,
            IServiceLocator serviceLocator, 
            IPlainAggregateRootRepository plainRepository,
            IAggregateLock aggregateLock,
            IAggregateRootCacheCleaner aggregateRootCacheCleaner,
            IEventStore eventStore)
            : base(eventSourcedRepository, eventBus, snapshooter, serviceLocator, plainRepository, 
                aggregateLock, aggregateRootCacheCleaner, eventStore) { }

        protected override void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            var commandDescriptor = new CommandDescriptor(command, origin, cancellationToken);

            this.AddToQueue(commandDescriptor);

            while (this.IsInQueue(commandDescriptor))
            {
                lock (this.lockObject)
                {
                    if (!this.IsOnTopOfQueue(commandDescriptor))
                        continue;

                    this.RemoveFromTopOfQueue(commandDescriptor);

                    base.ExecuteImpl(commandDescriptor.Command, commandDescriptor.Origin, commandDescriptor.CancellationToken);
                }
            }
        }

        private void AddToQueue(CommandDescriptor commandDescriptor)
        {
            this.queue.Enqueue(commandDescriptor);
        }

        private bool IsInQueue(CommandDescriptor commandDescriptor)
        {
            return this.queue.ToArray().Contains(commandDescriptor);
        }

        private bool IsOnTopOfQueue(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor topDescriptor;

            return this.queue.TryPeek(out topDescriptor) && topDescriptor == commandDescriptor;
        }

        private void RemoveFromTopOfQueue(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor removedDescriptor;

            var removeSucceeded = this.queue.TryDequeue(out removedDescriptor);

            if (!removeSucceeded)
                throw new CommandServiceException("Failed to remove top command from queue because queue is empty.");

            if (removedDescriptor != commandDescriptor)
                throw new CommandServiceException("Failed to remove top command from queue because not expected command was removed.");
        }
    }
}
