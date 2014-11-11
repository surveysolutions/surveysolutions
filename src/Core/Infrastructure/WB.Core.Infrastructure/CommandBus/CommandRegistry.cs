using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public static class CommandRegistry
    {
        private static readonly Dictionary<string, HandlerDescriptor> Handlers = new Dictionary<string, HandlerDescriptor>();

        public class AggregateRegistry<TAggregate>
            where TAggregate : IAggregateRoot
        {
            public AggregateRegistry<TAggregate> Add<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                CommandRegistry.Add(aggregateRootIdResolver, commandHandler);
                return this;
            }
        }

        private class HandlerDescriptor
        {
            public HandlerDescriptor(Type aggregateType, Func<ICommand, Guid> idResolver, Action<ICommand, IAggregateRoot> handler)
            {
                this.AggregateType = aggregateType;
                this.IdResolver = idResolver;
                this.Handler = handler;
            }

            public Type AggregateType { get; private set; }
            public Func<ICommand, Guid> IdResolver { get; private set; }
            public Action<ICommand, IAggregateRoot> Handler { get; private set; }
        }

        public static AggregateRegistry<TAggregate> For<TAggregate>()
            where TAggregate : IAggregateRoot
        {
            return new AggregateRegistry<TAggregate>();
        }

        public static void Add<TCommand, TAggregate>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
            where TCommand : ICommand
            where TAggregate : IAggregateRoot
        {
            string commandName = typeof(TCommand).Name;

            if (Handlers.ContainsKey(commandName))
                throw new ArgumentException(string.Format("Command {0} is already registered.", commandName));

            Handlers.Add(commandName, new HandlerDescriptor(
                typeof(TAggregate),
                command => aggregateRootIdResolver.Invoke((TCommand) command),
                (command, aggregate) => commandHandler.Invoke((TCommand) command, (TAggregate) aggregate)));
        }

        internal static bool Contains(ICommand command)
        {
            return Handlers.ContainsKey(command.GetType().Name);
        }

        public static Type GetAggregateRootType(ICommand command)
        {
            return Handlers[command.GetType().Name].AggregateType;
        }

        public static Func<ICommand, Guid> GetAggregateRootIdResolver(ICommand command)
        {
            return Handlers[command.GetType().Name].IdResolver;
        }

        public static Action<ICommand, IAggregateRoot> GetCommandHandler(ICommand command)
        {
            return Handlers[command.GetType().Name].Handler;
        }
    }
}