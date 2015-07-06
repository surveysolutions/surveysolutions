using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandHandlerConfiguration<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        private readonly List<Type> validators = new List<Type>();

        public CommandHandlerConfiguration<TAggregateRoot> ValidatedBy<T>() where T : ICommandValidator<TAggregateRoot>
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