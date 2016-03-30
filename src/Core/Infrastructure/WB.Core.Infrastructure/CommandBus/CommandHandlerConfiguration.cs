using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandHandlerConfiguration<TAggregate, TCommand>
        where TAggregate : IEventSourcedAggregateRoot
        where TCommand : ICommand
    {
        private readonly List<Type> validators = new List<Type>();

        public CommandHandlerConfiguration<TAggregate, TCommand> ValidatedBy<TValidator>() 
            where TValidator : ICommandValidator<TAggregate, TCommand>
        {
            this.validators.Add(typeof (TValidator));
            return this;
        }

        public List<Type> GetValidators()
        {
            return validators;
        }
    }
}