using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandHandlerConfiguration<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        private readonly List<Type> validators = new List<Type>();

        public CommandHandlerConfiguration<TAggregateRoot> ValidatedBy<T, TCommand>() 
            where T : ICommandValidator<TAggregateRoot, TCommand>
            where TCommand : ICommand
        {
            this.validators.Add(typeof (T));
            return this;
        }

        public List<Type> GetValidators()
        {
            return validators;
        }
    }
}