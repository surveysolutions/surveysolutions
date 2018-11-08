using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;


namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public class CommandService : ICommandService
    {
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly IPlainAggregateRootRepository plainRepository;
        private readonly IAggregateLock aggregateLock;
        private readonly ILiteEventBus eventBus;
        private readonly IAggregateSnapshotter snapshooter;
        private readonly IServiceLocator serviceLocator;
        private readonly IAggregateRootCacheCleaner aggregateRootCacheCleaner;
        private readonly EventBusSettings eventBusSettings;

        private static int executingCommandsCount = 0;
        private static readonly object executionCountLock = new object();
        private TaskCompletionSource<object> executionAwaiter = null;

        public CommandService(
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            ILiteEventBus eventBus,
            IAggregateSnapshotter snapshooter,
            IServiceLocator serviceLocator,
            IPlainAggregateRootRepository plainRepository,
            IAggregateLock aggregateLock, 
            IAggregateRootCacheCleaner aggregateRootCacheCleaner)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.eventBus = eventBus;
            this.snapshooter = snapshooter;
            this.serviceLocator = serviceLocator;
            this.plainRepository = plainRepository;
            this.aggregateLock = aggregateLock;
            this.aggregateRootCacheCleaner = aggregateRootCacheCleaner;
            
        }

        public Task ExecuteAsync(ICommand command, string origin, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Execute(command, origin, cancellationToken), cancellationToken);
        }

        public void Execute(ICommand command, string origin)
        {
            this.ExecuteImpl(command, origin, CancellationToken.None);
        }

        private void Execute(ICommand command, string origin, CancellationToken cancellationToken)
        {
            this.RegisterCommandExecution();

            try
            {
                this.ExecuteImpl(command, origin, cancellationToken);
            }
            finally
            {
                this.UnregisterCommandExecution();
            }
        }

        private void RegisterCommandExecution()
        {
            lock (executionCountLock)
            {
                executingCommandsCount++;
            }
        }

        private void UnregisterCommandExecution()
        {
            lock (executionCountLock)
            {
                executingCommandsCount--;

                if (executingCommandsCount > 0)
                    return;

                if (this.executionAwaiter != null)
                {
                    this.executionAwaiter.SetResult(new object());
                    this.executionAwaiter = null;
                }
            }
        }

        public Task WaitPendingCommandsAsync()
        {
            lock (executionCountLock)
            {
                if (executingCommandsCount == 0)
                    return Task.FromResult(null as object);

                if (this.executionAwaiter == null)
                {
                    this.executionAwaiter = new TaskCompletionSource<object>();
                }

                return this.executionAwaiter.Task;
            }
        }

        public bool HasPendingCommands => executingCommandsCount > 0;

        protected virtual void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            cancellationToken.ThrowIfCancellationRequested();

            if (!CommandRegistry.Contains(command))
                throw new CommandServiceException($"Unable to execute command {command.GetType().Name} because it is not registered.");

            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            AggregateKind aggregateKind = CommandRegistry.GetAggregateRootKind(command);
            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);
            IEnumerable<Action<IAggregateRoot, ICommand>> validators = CommandRegistry.GetValidators(command, this.serviceLocator);
            IEnumerable<Action<IAggregateRoot, ICommand>> postProcessors = CommandRegistry.GetPostProcessors(command, this.serviceLocator);
            IEnumerable<Action<IAggregateRoot, ICommand>> preProcessors = CommandRegistry.GetPreProcessors(command, this.serviceLocator);

            Guid aggregateId = aggregateRootIdResolver.Invoke(command);

            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (aggregateKind)
                {
                    case AggregateKind.EventSourced:
                        this.ExecuteEventSourcedCommand(command, origin, aggregateType, aggregateId, validators, preProcessors, postProcessors, commandHandler, cancellationToken);
                        break;

                    case AggregateKind.Plain:
                        this.ExecutePlainCommand(command, aggregateType, aggregateId, validators, preProcessors, postProcessors, commandHandler, cancellationToken);
                        break;

                    default:
                        throw new CommandServiceException($"Unable to execute command {command.GetType().Name} because it is registered to unknown aggregate root kind.");
                }
            });
        }

        private void ExecuteEventSourcedCommand(ICommand command,
            string origin,
            Type aggregateType,
            Guid aggregateId,
            IEnumerable<Action<IAggregateRoot, ICommand>> validators,
            IEnumerable<Action<IAggregateRoot, ICommand>> preProcessors,
            IEnumerable<Action<IAggregateRoot, ICommand>> postProcessors,
            Action<ICommand, IAggregateRoot> commandHandler,
            CancellationToken cancellationToken)
        {
            IEventSourcedAggregateRoot aggregate;

            if (CommandRegistry.IsStateless(command))
            {
                aggregate = this.eventSourcedRepository.GetStateless(aggregateType, aggregateId);
            }
            else
            {
                aggregate = this.eventSourcedRepository.GetLatest(aggregateType, aggregateId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (aggregate == null)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException($"Unable to execute not-constructing command {command.GetType().Name} because aggregate {aggregateId.FormatGuid()} does not exist.");

                aggregate = (IEventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateType);
                aggregate.SetId(aggregateId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> validator in validators)
            {
                validator.Invoke(aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> preProcessor in preProcessors)
            {
                preProcessor.Invoke(aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                commandHandler.Invoke(command, aggregate);
            }
            catch (OnEventApplyException)
            {
                // evict AR only if exception occured on event apply
                aggregateRootCacheCleaner.Evict(aggregateId);
                throw;
            }

            if (!aggregate.HasUncommittedChanges())
                return;

            //var eventStream = new UncommittedEventStream(origin, aggregate.GetUnCommittedChanges());

            var commitedEvents = this.eventBus.CommitUncommittedEvents(aggregate, origin);

            aggregate.MarkChangesAsCommitted();

            try
            {
                this.eventBus.PublishCommittedEvents(commitedEvents);

                foreach (Action<IAggregateRoot, ICommand> postProcessor in postProcessors)
                {
                    postProcessor.Invoke(aggregate, command);
                }
            }
            catch (Exception)
            {
                aggregateRootCacheCleaner.Evict(aggregateId);
                throw;
            }
            finally
            {
                this.snapshooter.CreateSnapshotIfNeededAndPossible(aggregate);
            }
        }

        private void ExecutePlainCommand(ICommand command,
            Type aggregateType, Guid aggregateId, IEnumerable<Action<IAggregateRoot, ICommand>> validators,
            IEnumerable<Action<IAggregateRoot, ICommand>> preProcessors,
            IEnumerable<Action<IAggregateRoot, ICommand>> postProcessors,
            Action<ICommand, IAggregateRoot> commandHandler, CancellationToken cancellationToken)
        {
            IPlainAggregateRoot aggregate = this.plainRepository.Get(aggregateType, aggregateId);

            if (aggregate == null)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException($"Unable to execute not-constructing command {command.GetType().Name} because aggregate {aggregateId.FormatGuid()} does not exist.");

                aggregate = (IPlainAggregateRoot)this.serviceLocator.GetInstance(aggregateType);
                aggregate.SetId(aggregateId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> validator in validators)
            {
                validator.Invoke(aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> preProcessor in preProcessors)
            {
                preProcessor.Invoke(aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            commandHandler.Invoke(command, aggregate);

            this.plainRepository.Save(aggregate);

            foreach (Action<IAggregateRoot, ICommand> postProcessor in postProcessors)
            {
                postProcessor.Invoke(aggregate, command);
            }
        }
    }
}
