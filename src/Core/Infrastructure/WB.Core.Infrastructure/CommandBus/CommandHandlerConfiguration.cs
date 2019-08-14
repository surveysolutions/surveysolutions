using System;
using System.Collections.Generic;
using System.Reflection;
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
        private readonly HashSet<Type> skippedValidationCommands = new HashSet<Type>();
        private readonly HashSet<Type> skippedPostProcessCommands = new HashSet<Type>();
        private readonly HashSet<Type> skippedPreProcessCommands = new HashSet<Type>();

        public CommandHandlerConfiguration<TAggregate, TCommand> ValidatedBy<TValidator>() 
            where TValidator : ICommandValidator<TAggregate, TCommand>
        {
            this.validators.Add(typeof (TValidator));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> SkipValidationFor<TSkipCommand>() where TSkipCommand : ICommand
        {
            this.skippedValidationCommands.Add(typeof(TSkipCommand));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> SkipPostProcessFor<TSkipCommand>() where TSkipCommand : ICommand
        {
            this.skippedPostProcessCommands.Add(typeof(TSkipCommand));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> SkipPreProcessFor<TSkipCommand>() where TSkipCommand : ICommand
        {
            this.skippedPreProcessCommands.Add(typeof(TSkipCommand));
            return this;
        }


        public CommandHandlerConfiguration<TAggregate, TCommand> PostProcessBy<TPostProcessor>()
            where TPostProcessor : ICommandPostProcessor<TAggregate, TCommand>
        {
            var item = typeof(TPostProcessor);
            this.postProcessors.Add(item);
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> PreProcessBy<TPostProcessor>()
            where TPostProcessor : ICommandPreProcessor<TAggregate, TCommand>
        {
            this.preProcessors.Add(typeof(TPostProcessor));
            return this;
        }

        public List<Type> GetValidators() => this.validators;

        public HashSet<Type> GetSkipValidationCommands() => this.skippedValidationCommands;

        public HashSet<Type> GetSkipPreProcessCommands() => this.skippedPreProcessCommands;

        public HashSet<Type> GetSkipPostProcessCommands() => this.skippedPostProcessCommands;

        public List<Type> GetPostProcessors() => this.postProcessors;

        public List<Type> GetPreProcessors() => this.preProcessors;
    }
}
