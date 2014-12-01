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
            public HandlerDescriptor(Type aggregateType, bool isInitializer, Func<ICommand, Guid> idResolver, Func<IAggregateRoot> constructor, Action<ICommand, IAggregateRoot> handler)
            {
                this.AggregateType = aggregateType;
                this.IsInitializer = isInitializer;
                this.IdResolver = idResolver;
                this.Constructor = constructor;
                this.Handler = handler;
            }

            public Type AggregateType { get; private set; }
            public bool IsInitializer { get; private set; }
            public Func<ICommand, Guid> IdResolver { get; private set; }
            public Func<IAggregateRoot> Constructor { get; private set; }
            public Action<ICommand, IAggregateRoot> Handler { get; private set; }
        }

        #endregion

        #region Fluent setup

        public class AggregateSetup<TAggregate>
            where TAggregate : IAggregateRoot, new()
        {
            public AggregateSetup<TAggregate> InitializesWith<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : ICommand
            {
                return InitializesWith(aggregateRootIdResolver, (command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateSetup<TAggregate> InitializesWith<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: true);
                return this;
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : ICommand
            {
                return Handles(aggregateRootIdResolver, (command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: false);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> ResolvesIdFrom<TAggregateCommand>(Func<TAggregateCommand, Guid> aggregateRootIdResolver)
                where TAggregateCommand : ICommand
            {
                return new AggregateWithCommandSetup<TAggregate, TAggregateCommand>(aggregateRootIdResolver);
            }
        }

        public class AggregateWithCommandSetup<TAggregate, TAggregateCommand>
            where TAggregate : IAggregateRoot, new()
            where TAggregateCommand : ICommand
        {
            private readonly Func<TAggregateCommand, Guid> aggregateRootIdResolver;

            public AggregateWithCommandSetup(Func<TAggregateCommand, Guid> aggregateRootIdResolver)
            {
                this.aggregateRootIdResolver = aggregateRootIdResolver;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : TAggregateCommand
            {
                return InitializesWith<TCommand>((command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Action<TCommand, TAggregate> commandHandler)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : TAggregateCommand
            {
                return Handles<TCommand>((command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Action<TCommand, TAggregate> commandHandler)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: false);
                return this;
            }
        }

        public static AggregateSetup<TAggregate> Setup<TAggregate>()
            where TAggregate : IAggregateRoot, new()
        {
            return new AggregateSetup<TAggregate>();
        }

        #endregion

        private static void Register<TCommand, TAggregate>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler, bool isInitializer)
            where TCommand : ICommand
            where TAggregate : IAggregateRoot, new()
        {
            string commandName = typeof (TCommand).Name;

            if (Handlers.ContainsKey(commandName))
                throw new ArgumentException(string.Format("Command {0} is already registered.", commandName));

            Handlers.Add(commandName, new HandlerDescriptor(
                typeof (TAggregate),
                isInitializer,
                command => aggregateRootIdResolver.Invoke((TCommand) command),
                () => new TAggregate(),
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

        internal static bool IsInitializer(ICommand command)
        {
            return Handlers[command.GetType().Name].IsInitializer;
        }

        internal static Func<ICommand, Guid> GetAggregateRootIdResolver(ICommand command)
        {
            return Handlers[command.GetType().Name].IdResolver;
        }

        internal static Func<IAggregateRoot> GetAggregateRootConstructor(ICommand command)
        {
            return Handlers[command.GetType().Name].Constructor;
        }

        internal static Action<ICommand, IAggregateRoot> GetCommandHandler(ICommand command)
        {
            return Handlers[command.GetType().Name].Handler;
        }
    }
}