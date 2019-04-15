using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class AddSharedPersonToQuestionnaire : QuestionnaireCommand
    {
        public AddSharedPersonToQuestionnaire(Guid questionnaireId, Guid personId, string emailOrLogin, ShareType shareType, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.PersonId = personId;
            this.EmailOrLogin = emailOrLogin;
            this.ShareType = shareType;
        }

        public ShareType ShareType { set; get; }

        public Guid PersonId { get; set; }
        public string EmailOrLogin { get; set; }
    }
}
