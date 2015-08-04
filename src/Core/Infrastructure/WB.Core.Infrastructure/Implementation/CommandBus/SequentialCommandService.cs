using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.Implementation.CommandBus
{
    internal class SequentialCommandService : CommandService
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

        public SequentialCommandService(IAggregateRootRepository repository, ILiteEventBus eventBus, IAggregateSnapshotter snapshooter,
            IServiceLocator serviceLocator)
            : base(repository, eventBus, snapshooter, serviceLocator) { }

        protected override void ExecuteImpl(ICommand command, string origin, bool handleInBatch, CancellationToken cancellationToken)
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

                    base.ExecuteImpl(commandDescriptor.Command, commandDescriptor.Origin, handleInBatch, commandDescriptor.CancellationToken);
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