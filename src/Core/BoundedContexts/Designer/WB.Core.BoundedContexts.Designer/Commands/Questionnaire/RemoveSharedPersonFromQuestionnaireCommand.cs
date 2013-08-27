using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "RemoveSharedPerson")]
    public class RemoveSharedPersonFromQuestionnaireCommand : QuestionnaireCommand
    {
        public RemoveSharedPersonFromQuestionnaireCommand(Guid questionnaireId, Guid personId, string email, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.Email = email;
            this.PersonId = personId;
        }

        public Guid PersonId { get; set; }
        public string Email { get; private set; }
    }
}