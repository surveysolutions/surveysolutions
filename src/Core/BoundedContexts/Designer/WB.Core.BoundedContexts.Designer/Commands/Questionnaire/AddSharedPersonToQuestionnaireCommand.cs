using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class AddSharedPersonToQuestionnaireCommand : QuestionnaireCommand
    {
        public AddSharedPersonToQuestionnaireCommand(Guid questionnaireId, Guid personId, string email, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.PersonId = personId;
            this.Email = email;
        }

        public Guid PersonId { get; set; }
        public string Email { get; private set; }
    }
}