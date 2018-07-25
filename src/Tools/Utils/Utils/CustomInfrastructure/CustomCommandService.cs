using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace Utils.CustomInfrastructure
{
    public class CustomCommandService : ICommandService
    {
        private readonly IServiceLocator serviceLocator;

        IEventSourcedAggregateRoot aggregate = null;


        public CustomCommandService(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Task ExecuteAsync(ICommand command, string origin, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Execute(command, origin, cancellationToken), cancellationToken);
        }

        public Task WaitPendingCommandsAsync()
        {
            throw new NotImplementedException();
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
        }

        private void UnregisterCommandExecution()
        {
        }

        public bool HasPendingCommands => false;

        protected virtual void ExecuteImpl(ICommand command, string origin, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            cancellationToken.ThrowIfCancellationRequested();

            if (command is DeleteInterviewCommand)
            {
                this.aggregate = null;
                return;
            }


            if (!CommandRegistry.Contains(command))
                throw new CommandServiceException(
                    $"Unable to execute command {command.GetType().Name} because it is not registered.");
            
            Type aggregateType = CommandRegistry.GetAggregateRootType(command);
            Func<ICommand, Guid> aggregateRootIdResolver = CommandRegistry.GetAggregateRootIdResolver(command);
            Action<ICommand, IAggregateRoot> commandHandler = CommandRegistry.GetCommandHandler(command);
            IEnumerable<Action<IAggregateRoot, ICommand>> validators = CommandRegistry.GetValidators(command, this.serviceLocator);
            IEnumerable<Action<IAggregateRoot, ICommand>> preProcessors = CommandRegistry.GetPreProcessors(command, this.serviceLocator);

            Guid aggregateId = aggregateRootIdResolver.Invoke(command);


            cancellationToken.ThrowIfCancellationRequested();

            if (this.aggregate == null || this.aggregate.EventSourceId != aggregateId)
            {
                if (!CommandRegistry.IsInitializer(command))
                    throw new CommandServiceException(
                        $"Unable to execute not-constructing command {command.GetType().Name} because aggregate {aggregateId.FormatGuid()} does not exist.");
                
                this.aggregate = (IEventSourcedAggregateRoot) this.serviceLocator.GetInstance(aggregateType);
                this.aggregate.SetId(aggregateId);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> validator in validators)
            {
                validator.Invoke(this.aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Action<IAggregateRoot, ICommand> preProcessor in preProcessors)
            {
                preProcessor.Invoke(this.aggregate, command);
            }

            cancellationToken.ThrowIfCancellationRequested();

            commandHandler.Invoke(command, this.aggregate);

            if (!this.aggregate.HasUncommittedChanges())
                return;

            var eventStream = new UncommittedEventStream(origin, this.aggregate.GetUnCommittedChanges());
            
            this.aggregate.MarkChangesAsCommitted();
        }
    }
}