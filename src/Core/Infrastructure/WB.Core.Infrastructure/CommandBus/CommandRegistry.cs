using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public static class CommandRegistry
    {
        #region Handlers

        private static readonly Dictionary<string, HandlerDescriptor> Handlers = new Dictionary<string, HandlerDescriptor>();

        private class HandlerDescriptor
        {
            public HandlerDescriptor(
                Type aggregateType, 
                bool isInitializer, 
                Func<ICommand, Guid> idResolver,
                Action<ICommand, IAggregateRoot> handler,
                IEnumerable<Type> validators)
            {
                this.AggregateType = aggregateType;
                this.IsEventSourced = false;
                this.IsInitializer = isInitializer;
                this.IdResolver = idResolver;
                this.Handler = handler;
                this.Validators = validators != null ? new List<Type>(validators) : new List<Type>();                
            }

            public Type AggregateType { get; }
            public bool IsEventSourced { get; }
            public bool IsInitializer { get; }
            public Func<ICommand, Guid> IdResolver { get; }
            public Action<ICommand, IAggregateRoot> Handler { get; }
            public List<Type> Validators { get; }

            public void AppendValidators(List<Type> validators)
            {
                Validators.AddRange(validators);
            }
        }

        #endregion

        #region Fluent setup

        public class AggregateSetup<TAggregate>
            where TAggregate : IAggregateRoot
        {
            public AggregateSetup<TAggregate> InitializesWith<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : ICommand
            {
                return InitializesWith(aggregateRootIdResolver, (command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateSetup<TAggregate> InitializesWith<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: true, configurer: null);
                return this;
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : ICommand
            {
                return Handles(aggregateRootIdResolver, (command, aggregate) => commandHandler(aggregate)(command), configurer: null);
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(Func<TCommand, Guid> aggregateRootIdResolver, Action<TCommand, TAggregate> commandHandler)
                where TCommand : ICommand
            {
                return Handles(aggregateRootIdResolver, commandHandler, configurer: null);
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver, 
                Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: false, configurer: configurer);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> ResolvesIdFrom<TAggregateCommand>(Func<TAggregateCommand, Guid> aggregateRootIdResolver)
                where TAggregateCommand : ICommand
            {
                return new AggregateWithCommandSetup<TAggregate, TAggregateCommand>(aggregateRootIdResolver);
            }
        }

        public class AggregateWithCommandSetup<TAggregate, TAggregateCommand>
            where TAggregate : IAggregateRoot
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

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Func<TAggregate, Action<TCommand>> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : TAggregateCommand
            {
                return InitializesWith<TCommand>((command, aggregate) => commandHandler(aggregate)(command), configurer);
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Action<TCommand, TAggregate> commandHandler)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true, configurer: null);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true, configurer: configurer);
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
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: false, configurer: null);
                return this;
            }
        }

        public static AggregateSetup<TAggregate> Setup<TAggregate>()
            where TAggregate : IAggregateRoot
        {
            return new AggregateSetup<TAggregate>();
        }

        #endregion

        private static void Register<TCommand, TAggregate>(Func<TCommand, Guid> aggregateRootIdResolver, 
            Action<TCommand, TAggregate> commandHandler, 
            bool isInitializer,
            Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
            where TCommand : ICommand
            where TAggregate : IAggregateRoot
        {
            string commandName = typeof (TCommand).Name;

            if (Handlers.ContainsKey(commandName))
                throw new ArgumentException($"Command {commandName} is already registered.");

            var configuration = new CommandHandlerConfiguration<TAggregate, TCommand>();
            configurer?.Invoke(configuration);

            Handlers.Add(commandName, new HandlerDescriptor(
                typeof (TAggregate),
                isInitializer: isInitializer,
                idResolver: command => aggregateRootIdResolver.Invoke((TCommand) command),
                handler: (command, aggregate) => commandHandler.Invoke((TCommand) command, (TAggregate) aggregate),
                validators: configuration.GetValidators()));
        }

        internal static bool Contains(ICommand command)
            => Handlers.ContainsKey(command.GetType().Name);

        private static HandlerDescriptor GetHandlerDescriptor(ICommand command)
            => Handlers[command.GetType().Name];

        internal static Type GetAggregateRootType(ICommand command)
            => GetHandlerDescriptor(command).AggregateType;

        internal static bool IsInitializer(ICommand command)
            => GetHandlerDescriptor(command).IsInitializer;

        internal static Func<ICommand, Guid> GetAggregateRootIdResolver(ICommand command)
            => GetHandlerDescriptor(command).IdResolver;

        internal static Action<ICommand, IAggregateRoot> GetCommandHandler(ICommand command)
            => GetHandlerDescriptor(command).Handler;

        internal static bool IsAggregateEventSourced(ICommand command)
            => GetHandlerDescriptor(command).IsEventSourced;

        public static IEnumerable<Action<IAggregateRoot, ICommand>> GetValidators(ICommand command, IServiceLocator serviceLocator)
        {
            var handlerDescriptor = GetHandlerDescriptor(command);

            return handlerDescriptor.Validators.Select(
                validatorType => GetValidatingAction(validatorType, handlerDescriptor.AggregateType, command.GetType(), serviceLocator));
        }

        private static Action<IAggregateRoot, ICommand> GetValidatingAction(Type validatorType, Type aggregateType, Type commandType, IServiceLocator serviceLocator)
        {
            object validatorInstance = serviceLocator.GetInstance(validatorType);
            MethodInfo validatingMethod = validatorType.GetMethod("Validate", new[] { aggregateType, commandType });

            if (validatingMethod == null)
                throw new CommandRegistryException(string.Format("Unable to resolve validating method of validator {0} for command {1} and aggregate {2}.",
                    validatorType.Name, commandType.Name, aggregateType.Name));

            return (aggregate, command) =>
            {
                try
                {
                    validatingMethod.Invoke(validatorInstance, new object[] { aggregate, command });
                }
                catch (TargetInvocationException exception)
                {
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                }
            };
        }

        public static void Configure<TAggregate, TCommand>(Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
            where TAggregate : IAggregateRoot
            where TCommand : ICommand
        {
            var configuration = new CommandHandlerConfiguration<TAggregate, TCommand>();

            configurer.Invoke(configuration);

            Handlers[typeof(TCommand).Name].AppendValidators(configuration.GetValidators());
        }
    }
}