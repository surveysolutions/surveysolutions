using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupViewInputModel
    {
        public GroupViewInputModel(Guid publicKey, Guid questionnaireId)
        {
            PublicKey = publicKey;
            QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; set; }
        public Guid PublicKey { get; private set; }
    }
}
