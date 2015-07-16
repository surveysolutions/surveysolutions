using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class AddSharedPersonToQuestionnaireCommand : QuestionnaireCommand
    {
        public AddSharedPersonToQuestionnaireCommand(Guid questionnaireId, Guid personId, string email, ShareType shareType, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.PersonId = personId;
            this.Email = email;
            this.ShareType = shareType;
        }

        public ShareType ShareType { set; get; }

        public Guid PersonId { get; set; }
        public string Email { get; private set; }
    }
}