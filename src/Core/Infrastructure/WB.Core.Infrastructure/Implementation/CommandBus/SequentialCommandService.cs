using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            public CommandDescriptor(ICommand command, string origin)
            {
                Command = command;
                Origin = origin;
            }

            public ICommand Command { get; private set; }
            public string Origin { get; private set; }
        }

        private readonly ConcurrentQueue<CommandDescriptor> commandsQueue = new ConcurrentQueue<CommandDescriptor>();
        private static readonly object LockObject = new object();

        public SequentialCommandService(IAggregateRootRepository repository, ILiteEventBus eventBus, IAggregateSnapshotter snapshooter)
            : base(repository, eventBus, snapshooter) {}

        public override void Execute(ICommand command, string origin)
        {
            var commandDescriptor = new CommandDescriptor(command, origin);

            this.commandsQueue.Enqueue(commandDescriptor);

            lock (LockObject)
            {
                this.RunCommandsFromQueueUntil(commandDescriptor);
            }
        }

        private void RunCommandsFromQueueUntil(CommandDescriptor expectedCommandDescriptor)
        {
            if (!this.commandsQueue.Contains(expectedCommandDescriptor))
                return;

            CommandDescriptor commandDescriptor;
            while (this.commandsQueue.TryDequeue(out commandDescriptor))
            {
                base.Execute(commandDescriptor.Command, commandDescriptor.Origin);

                if (commandDescriptor == expectedCommandDescriptor)
                    return;
            }
        }
    }
}