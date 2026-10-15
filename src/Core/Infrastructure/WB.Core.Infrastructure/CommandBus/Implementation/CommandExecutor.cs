using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IEventSourcedAggregateRootRepository eventSourcedRepository;
        private readonly ILiteEventBus eventBus;
        private readonly IServiceLocator serviceLocator;
        private readonly IPlainAggregateRootRepository plainRepository;
        private readonly IAggregateRootCache aggregateRootCache;
        private readonly ICommandsMonitoring commandsMonitoring;
        private readonly IAggregateRootPrototypePromoterService promoterService;
        private readonly IAggregateRootPrototypeService prototypeService;

        public CommandExecutor(
            IEventSourcedAggregateRootRepository eventSourcedRepository,
            ILiteEventBus eventBus,
            IServiceLocator serviceLocator,
            IPlainAggregateRootRepository plainRepository,
            IAggregateRootCache aggregateRootCache,
            ICommandsMonitoring commandsMonitoring,
            IAggregateRootPrototypePromoterService promoterService,
            IAggregateRootPrototypeService prototypeService)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.eventBus = eventBus;
            this.serviceLocator = serviceLocator;
            this.plainRepository = plainRepository;
            this.aggregateRootCache = aggregateRootCache;
            this.commandsMonitoring = commandsMonitoring;
            this.promoterService = promoterService;
            this.prototypeService = prototypeService;
        }

        public void ExecuteCommand(ICommand command, string origin, CancellationToken cancellationToken, Guid aggregateId)
        {
            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            AggregateKind aggregateKind = CommandRegistry.GetAggregateRootKind(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);
            IEnumerable<Action<IAggregateRoot, ICommand>> validators = CommandRegistry.GetValidators(command, this.serviceLocator);
            IEnumerable<Action<IAggregateRoot, ICommand>> postProcessors = CommandRegistry.GetPostProcessors(command, this.serviceLocator);
            IEnumerable<Action<IAggregateRoot, ICommand>> preProcessors = CommandRegistry.GetPreProcessors(command, this.serviceLocator);

            cancellationToken.ThrowIfCancellationRequested();

            var sw = Stopwatch.StartNew();
            switch (aggregateKind)
            {
                case AggregateKind.EventSourced:
                    this.ExecuteEventSourcedCommand(command, origin, aggregateType, aggregateId, validators, preProcessors,
                        postProcessors, commandHandler, cancellationToken);
                    break;

                case AggregateKind.Plain:
                    this.ExecutePlainCommand(command, aggregateType, aggregateId, validators, preProcessors, postProcessors,
                        commandHandler, cancellationToken);
                    break;

                default:
                    throw new CommandServiceException(
                        $"Unable to execute command {command.GetType().Name} because it is registered to unknown aggregate root kind.");
            }

            sw.Stop();
            this.commandsMonitoring.Report(command.GetType().Name, sw.Elapsed);
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

            bool mustPromotePrototype = false;

            if (aggregate == null)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException(
                        $"Unable to execute not-constructing command {command.GetType().Name} " +
                                $"because aggregate {aggregateId.FormatGuid()} does not exist.");

                aggregate = (IEventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateType);
                aggregate.SetId(aggregateId);
            }
            else if (prototypeService.IsPrototype(aggregateId))
            {
                mustPromotePrototype = true;
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
                // evict AR only if exception occurred on event apply
                aggregateRootCache.EvictAggregateRoot(aggregateId);
                throw;
            }
            catch (Exception)
            {
                aggregate.DiscardChanges();
                throw;
            }

            if (!aggregate.HasUncommittedChanges())
                return;

            var committedEvents = this.eventBus.CommitUncommittedEvents(aggregate, origin);

            aggregate.MarkChangesAsCommitted();

            try
            {
                this.eventBus.PublishCommittedEvents(committedEvents);

                if (mustPromotePrototype)
                {
                    promoterService.MaterializePrototypeIfRequired(aggregateId);
                }

                foreach (Action<IAggregateRoot, ICommand> postProcessor in postProcessors)
                {
                    postProcessor.Invoke(aggregate, command);
                }
            }
            catch (Exception)
            {
                aggregateRootCache.EvictAggregateRoot(aggregateId);
                throw;
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
