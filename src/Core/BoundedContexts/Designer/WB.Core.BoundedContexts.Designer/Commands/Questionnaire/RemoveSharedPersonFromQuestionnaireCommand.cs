using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "RemoveSharedPerson")]
    public class RemoveSharedPersonFromQuestionnaireCommand : QuestionnaireCommand
    {
        public RemoveSharedPersonFromQuestionnaireCommand(Guid questionnaireId, Guid personId)
            : base(questionnaireId)
        {
            this.PersonId = personId;
        }

        public Guid PersonId { get; private set; }
    }
}