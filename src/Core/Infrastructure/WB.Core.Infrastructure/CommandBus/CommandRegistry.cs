using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus.Implementation;

namespace WB.Core.Infrastructure.CommandBus
{
    public static class CommandRegistry
    {
        #region Handlers

        private static readonly Dictionary<string, HandlerDescriptor> Handlers = new Dictionary<string, HandlerDescriptor>();

        private class HandlerDescriptor
        {
            public HandlerDescriptor(Type aggregateType, bool isInitializer, bool isStateless, Func<ICommand, Guid> idResolver, Action<ICommand, IAggregateRoot> handler, IEnumerable<Type> validators, IEnumerable<Type> postProcessors, IEnumerable<Type> preProcessors)
            {
                this.AggregateType = aggregateType;
                this.AggregateKind = DetermineAggregateKind(aggregateType);
                this.IsInitializer = isInitializer;
                this.IsStateless = isStateless;
                this.IdResolver = idResolver;
                this.Handler = handler;
                this.Validators = validators != null ? new List<Type>(validators) : new List<Type>();
                this.PostProcessors = postProcessors != null ? new List<Type>(postProcessors) : new List<Type>();
                this.PreProcessors = preProcessors != null ? new List<Type>(preProcessors) : new List<Type>();
            }

            public Type AggregateType { get; }
            public AggregateKind AggregateKind { get; }
            public bool IsInitializer { get; }
            public bool IsStateless { get; }
            public Func<ICommand, Guid> IdResolver { get; }
            public Action<ICommand, IAggregateRoot> Handler { get; }
            public List<Type> Validators { get; }
            public List<Type> PostProcessors { get; }
            public List<Type> PreProcessors { get; }

            public void AppendValidators(List<Type> validators) => this.Validators.AddRange(validators);
            public void AppendPostProcessors(List<Type> postProcessors) => this.PostProcessors.AddRange(postProcessors);

            private static AggregateKind DetermineAggregateKind(Type aggregateType)
            {
                if (aggregateType.Implements<IEventSourcedAggregateRoot>())
                    return AggregateKind.EventSourced;

                if (aggregateType.Implements<IPlainAggregateRoot>())
                    return AggregateKind.Plain;

                throw new ArgumentException($"Aggregate {aggregateType} should implement interface IEventSourcedAggregateRoot or IPlainAggregateRoot.");
            }
        }

        #endregion

        #region Fluent setup

        public class AggregateSetup<TAggregate>
            where TAggregate : IAggregateRoot
        {
            public AggregateSetup<TAggregate> InitializesWith<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver,
                Func<TAggregate, Action<TCommand>> getCommandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null)
                where TCommand : ICommand
                => this.InitializesWith(
                    aggregateRootIdResolver,
                    (command, aggregate) => getCommandHandler(aggregate).Invoke(command),
                    configurer);

            public AggregateSetup<TAggregate> InitializesWith<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver,
                Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: true, isStateless:false, configurer: configurer);
                return this;
            }

            public AggregateSetup<TAggregate> Handles<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver,
                Func<TAggregate, Action<TCommand>> getCommandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null) 
                where TCommand : ICommand
                => this.Handles(
                    aggregateRootIdResolver,
                    (command, aggregate) => getCommandHandler(aggregate).Invoke(command),
                    configurer);

            public AggregateSetup<TAggregate> Handles<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver, 
                Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: false, isStateless: false, configurer: configurer);
                return this;
            }

            public AggregateSetup<TAggregate> StatelessHandles<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver,
                Func<TAggregate, Action<TCommand>> getCommandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null)
                where TCommand : ICommand
                => this.StatelessHandles(
                    aggregateRootIdResolver,
                    (command, aggregate) => getCommandHandler(aggregate).Invoke(command),
                    configurer);

            public AggregateSetup<TAggregate> StatelessHandles<TCommand>(
                Func<TCommand, Guid> aggregateRootIdResolver, 
                Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer = null)
                where TCommand : ICommand
            {
                Register(aggregateRootIdResolver, commandHandler, isInitializer: false, isStateless: true, configurer: configurer);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> ResolvesIdFrom<TAggregateCommand>(
                Func<TAggregateCommand, Guid> aggregateRootIdResolver)
                where TAggregateCommand : ICommand
                => new AggregateWithCommandSetup<TAggregate, TAggregateCommand>(aggregateRootIdResolver);
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
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true, isStateless:false, configurer: null);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> InitializesWith<TCommand>(Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true, isStateless: false, configurer: configurer);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Func<TAggregate, Action<TCommand>> commandHandler)
                where TCommand : TAggregateCommand
            {
                return Handles<TCommand>((command, aggregate) => commandHandler(aggregate)(command));
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Func<TAggregate, Action<TCommand>> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), (command, aggregate) => commandHandler(aggregate)(command), isInitializer: true, isStateless: false, configurer: configurer);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Action<TCommand, TAggregate> commandHandler,
                Action<CommandHandlerConfiguration<TAggregate, TCommand>> configurer)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: true, isStateless: false, configurer: configurer);
                return this;
            }

            public AggregateWithCommandSetup<TAggregate, TAggregateCommand> Handles<TCommand>(Action<TCommand, TAggregate> commandHandler)
                where TCommand : TAggregateCommand
            {
                Register(command => this.aggregateRootIdResolver.Invoke(command), commandHandler, isInitializer: false, isStateless: false, configurer: null);
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
            bool isStateless,
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
                isStateless: isStateless,
                idResolver: command => aggregateRootIdResolver.Invoke((TCommand) command),
                handler: (command, aggregate) => commandHandler.Invoke((TCommand) command, (TAggregate) aggregate),
                validators: configuration.GetValidators(),
                postProcessors: configuration.GetPostProcessors(),
                preProcessors: configuration.GetPreProcessors()));
        }

        public static bool Contains(ICommand command)
            => Handlers.ContainsKey(command.GetType().Name);

        private static HandlerDescriptor GetHandlerDescriptor(ICommand command)
            => Handlers[command.GetType().Name];

        public static Type GetAggregateRootType(ICommand command)
            => GetHandlerDescriptor(command).AggregateType;

        internal static AggregateKind GetAggregateRootKind(ICommand command)
            => GetHandlerDescriptor(command).AggregateKind;

        public static bool IsInitializer(ICommand command)
            => GetHandlerDescriptor(command).IsInitializer;

        internal static bool IsStateless(ICommand command)
            => GetHandlerDescriptor(command).IsStateless;

        public static Func<ICommand, Guid> GetAggregateRootIdResolver(ICommand command)
            => GetHandlerDescriptor(command).IdResolver;

        public static Action<ICommand, IAggregateRoot> GetCommandHandler(ICommand command)
            => GetHandlerDescriptor(command).Handler;

        public static IEnumerable<Action<IAggregateRoot, ICommand>> GetValidators(ICommand command, IServiceLocator serviceLocator)
        {
            var handlerDescriptor = GetHandlerDescriptor(command);

            return handlerDescriptor.Validators.Select(
                validatorType => GetValidatingAction(validatorType, handlerDescriptor.AggregateType, command.GetType(), serviceLocator));
        }

        public static IEnumerable<Action<IAggregateRoot, ICommand>> GetPostProcessors(ICommand command, IServiceLocator serviceLocator)
        {
            var handlerDescriptor = GetHandlerDescriptor(command);
            return GetProcessors(handlerDescriptor.PostProcessors, command, serviceLocator);
        }
        public static IEnumerable<Action<IAggregateRoot, ICommand>> GetPreProcessors(ICommand command, IServiceLocator serviceLocator)
        {
            var handlerDescriptor = GetHandlerDescriptor(command);
            return GetProcessors(handlerDescriptor.PreProcessors, command, serviceLocator);
        }
        public static IEnumerable<Action<IAggregateRoot, ICommand>> GetProcessors(List<Type> processors, ICommand command, IServiceLocator serviceLocator)
        {
            var handlerDescriptor = GetHandlerDescriptor(command);

            return processors.Select(
                processorType => GetProcessingAction(processorType, handlerDescriptor.AggregateType, command.GetType(), serviceLocator));
        }

        private static Action<IAggregateRoot, ICommand> GetValidatingAction(Type validatorType, Type aggregateType, Type commandType, IServiceLocator serviceLocator)
        {
            object validatorInstance = serviceLocator.GetInstance(validatorType);

            if (validatorInstance == null)
                throw new CommandRegistryException($"Unable to get instance of validator {validatorType.Name} for command {commandType.Name} and aggregate {aggregateType.Name}.");

            MethodInfo validatingMethod = validatorType.GetTypeInfo().GetMethod("Validate", new[] { aggregateType, commandType });

            if (validatingMethod == null)
                throw new CommandRegistryException($"Unable to resolve validating method of validator {validatorType.Name} for command {commandType.Name} and aggregate {aggregateType.Name}.");

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

        private static Action<IAggregateRoot, ICommand> GetProcessingAction(Type processorType, Type aggregateType, Type commandType, IServiceLocator serviceLocator)
        {
            object postProcessorInstance = serviceLocator.GetInstance(processorType);

            if (postProcessorInstance == null)
                throw new CommandRegistryException($"Unable to get instance of post processor {processorType.Name} for command {commandType.Name} and aggregate {aggregateType.Name}.");

            MethodInfo processMethod = processorType.GetTypeInfo().GetMethod("Process", new[] { aggregateType, commandType });

            if (processMethod == null)
                throw new CommandRegistryException($"Unable to resolve process method of post processor {processorType.Name} for command {commandType.Name} and aggregate {aggregateType.Name}.");

            return (aggregate, command) =>
            {
                try
                {
                    processMethod.Invoke(postProcessorInstance, new object[] { aggregate, command });
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

            var commandType = typeof(TCommand);
            var commandName = commandType.Name;
            if (Handlers.ContainsKey(commandName))
            {
                Handlers[commandName].AppendValidators(configuration.GetValidators());
                Handlers[commandName].AppendPostProcessors(configuration.GetPostProcessors());
                return;
            }

            foreach (var handlersKey in Handlers.Keys)
            {
                var commandDescriptor = Handlers[handlersKey];
                var registeredExistingCommand = commandDescriptor.Handler.Target.GetType().GetGenericArguments().FirstOrDefault();

                if (registeredExistingCommand == null) continue;

                if(!commandType.IsAssignableFrom(registeredExistingCommand)) continue;

                if (!configuration.GetSkipCommands().Contains(registeredExistingCommand))
                {
                    commandDescriptor.AppendValidators(configuration.GetValidators());
                }

                if (!configuration.GetSkipPostProcessCommands().Contains(registeredExistingCommand))
                {
                    commandDescriptor.AppendPostProcessors(configuration.GetPostProcessors());
                }
            }
        }
    }
}
