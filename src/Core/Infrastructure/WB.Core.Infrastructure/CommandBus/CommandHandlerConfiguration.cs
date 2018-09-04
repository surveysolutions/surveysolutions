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
        private readonly HashSet<Type> skippedCommands = new HashSet<Type>();
        private readonly HashSet<Type> skippedPostProcessCommands = new HashSet<Type>();

        public CommandHandlerConfiguration<TAggregate, TCommand> ValidatedBy<TValidator>() 
            where TValidator : ICommandValidator<TAggregate, TCommand>
        {
            this.validators.Add(typeof (TValidator));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> SkipValidationFor<TSkipCommand>() where TSkipCommand : ICommand
        {
            this.skippedCommands.Add(typeof(TSkipCommand));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> SkipPostProcessFor<TSkipCommand>() where TSkipCommand : ICommand
        {
            this.skippedPostProcessCommands.Add(typeof(TSkipCommand));
            return this;
        }

        public CommandHandlerConfiguration<TAggregate, TCommand> PostProcessBy<TPostProcessor>()
            where TPostProcessor : ICommandPostProcessor<TAggregate, TCommand>
        {
            var item = typeof(TPostProcessor);
            var preprocessorAttribute = item.GetCustomAttribute<RequiresPreprocessorAttribute>();
            if (preprocessorAttribute != null)
            {
                this.preProcessors.Add(preprocessorAttribute.PreProcessor);
            }

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

        public HashSet<Type> GetSkipCommands() => this.skippedCommands;

        public HashSet<Type> GetSkipPostProcessCommands() => this.skippedPostProcessCommands;

        public List<Type> GetPostProcessors() => this.postProcessors;

        public List<Type> GetPreProcessors() => this.preProcessors;
    }
}
