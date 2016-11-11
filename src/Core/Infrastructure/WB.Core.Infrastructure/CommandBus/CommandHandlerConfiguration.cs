using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandHandlerConfiguration<TAggregate, TCommand>
        where TAggregate : IAggregateRoot
        where TCommand : ICommand
    {
        private readonly List<Type> validators = new List<Type>();
        private readonly List<Type> postProcessors = new List<Type>();
        private readonly List<Type> preProcessors = new List<Type>();

        public CommandHandlerConfiguration<TAggregate, TCommand> ValidatedBy<TValidator>() 
            where TValidator : ICommandValidator<TAggregate, TCommand>
        {
            this.validators.Add(typeof (TValidator));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> PostProcessBy<TPostProcessor>()
            where TPostProcessor : ICommandPostProcessor<TAggregate, TCommand>
        {
            this.postProcessors.Add(typeof(TPostProcessor));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> PreProcessBy<TPostProcessor>()
            where TPostProcessor : ICommandPreProcessor<TAggregate, TCommand>
        {
            this.preProcessors.Add(typeof(TPostProcessor));
            return this;
        }

        public List<Type> GetValidators() => this.validators;

        public List<Type> GetPostProcessors() => this.postProcessors;

        public List<Type> GetPreProcessors() => this.preProcessors;
    }
}