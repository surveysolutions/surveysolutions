using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireCommand : QuestionnaireCommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId)
            : base(responsibleId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}