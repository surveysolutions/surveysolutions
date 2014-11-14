using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public abstract class QuestionnaireCommand : CommandBase
    {
        protected QuestionnaireCommand(Guid commandIdentifier, Guid questionnaireId)
            : base(commandIdentifier)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}