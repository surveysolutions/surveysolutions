using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireCommand : CommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}