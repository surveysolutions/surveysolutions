using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupViewInputModel
    {
        public GroupViewInputModel(Guid publicKey, string questionnaireId)
        {
            PublicKey = publicKey;
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }

        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; private set; }
    }
}
