using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "AddSharedPerson")]
    public class AddSharedPersonToQuestionnaireCommand : QuestionnaireCommand
    {
        public AddSharedPersonToQuestionnaireCommand(Guid questionnaireId, Guid personId, string email, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.PersonId = personId;
            this.Email = email;
        }

        public Guid PersonId { get; private set; }
        public string Email { get; private set; }
    }
}