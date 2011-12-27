using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionViewInputModel
    {
        public QuestionViewInputModel(Guid publicKey, string questionnaireId)
        {
            PublicKey = publicKey;
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }

        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; private set; }
    }
}
