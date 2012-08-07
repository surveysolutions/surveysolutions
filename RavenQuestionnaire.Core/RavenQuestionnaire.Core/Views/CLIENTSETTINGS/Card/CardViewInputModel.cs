using System;
using System.Security.Cryptography.X509Certificates;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class CardViewInputModel
    {
        public CardViewInputModel(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            QuestionKey = publicKey;
            ImageKey = imageKey;
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }

        public string QuestionnaireId { get; set; }
        public Guid QuestionKey { get; private set; }
        public Guid ImageKey { get; private set; }
    }
}
