using System;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionViewInputModel
    {
        public QuestionViewInputModel(Guid publicKey, Guid questionnaireId)
        {
            PublicKey = publicKey;
            QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; set; }
        public Guid PublicKey { get; private set; }
    }
}
