using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public static class CommandRegistry
    {
        #region Handlers

        private static readonly Dictionary<string, HandlerDescriptor> Handlers = new Dictionary<string, HandlerDescriptor>();

        private class HandlerDescriptor
        {
            public HandlerDescriptor(Type aggregateType, bool isConstructor, Func<ICommand, Guid> idResolver, Action<ICommand, IAggregateRoot> handler)
            {
                this.IsConstructor = isConstructor;
                this.AggregateType = aggregateType;
                this.IdResolver = idResolver;
                this.Handler = handler;
            }

            public Type AggregateType { get; private set; }
            public bool IsConstructor { get; set; }
            public Func<ICommand, Guid> IdResolver { get; private set; }
            public Action<ICommand, IAggregateRoot> Handler { get; private set; }
        }

        #endregion

        #region Fluent setup

        public class AggregateSetup<TAggregate>
            where TAggregate : IAggregateRoot
        {
            public AggregateSetup<TAggregate> InitializedWith<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isConstructor: true);
                return this;
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isConstructor: false);
                return this;
            }
        }

        public static AggregateSetup<TAggregate> Setup<TAggregate>()
            where TAggregate : IAggregateRoot
        {
            return new AggregateSetup<TAggregate>();
        }

        #endregion

        private static void Register<TCommand, TAggregate>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler, bool isConstructor)
            where TCommand : ICommand where TAggregate : IAggregateRoot
        {
            string commandName = typeof (TCommand).Name;

            if (Handlers.ContainsKey(commandName))
                throw new ArgumentException(string.Format("Command {0} is already registered.", commandName));

            Handlers.Add(commandName, new HandlerDescriptor(
                typeof (TAggregate),
                isConstructor,
                command => aggregateRootIdResolver.Invoke((TCommand) command),
                (command, aggregate) => commandHandler.Invoke((TCommand) command, (TAggregate) aggregate)));
        }

        internal static bool Contains(ICommand command)
        {
            return Handlers.ContainsKey(command.GetType().Name);
        }

        internal static Type GetAggregateRootType(ICommand command)
        {
            return Handlers[command.GetType().Name].AggregateType;
        }

        internal static bool IsConstructor(ICommand command)
        {
            return Handlers[command.GetType().Name].IsConstructor;
        }

        internal static Func<ICommand, Guid> GetAggregateRootIdResolver(ICommand command)
        {
            return Handlers[command.GetType().Name].IdResolver;
        }

        internal static Action<ICommand, IAggregateRoot> GetCommandHandler(ICommand command)
        {
            return Handlers[command.GetType().Name].Handler;
        }
    }
}