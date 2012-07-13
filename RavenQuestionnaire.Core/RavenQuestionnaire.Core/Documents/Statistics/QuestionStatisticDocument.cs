#region

using System;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

#endregion

namespace RavenQuestionnaire.Core.Documents.Statistics
{
    public class QuestionStatisticDocument
    {
        private ICompleteQuestion completeQuestion;

        public QuestionStatisticDocument()
        {
        }

        public QuestionStatisticDocument(ICompleteQuestion question, Guid gropPublicKey, Guid? gropPropagationPublicKey, Guid screenPublicKey)
        {
            PublicKey = question.PublicKey;
            GroupPublicKey = gropPublicKey;
            GroupPropagationPublicKey = gropPropagationPublicKey;
            ScreenPublicKey = screenPublicKey;
            QuestionText = question.QuestionText;
            QuestionType = question.QuestionType;
        
            if (question.Answer!=null)
            {
           //     AnswerValue = answer.AnswerText?? answer.AnswerValue;
                AnswerDate = question.AnswerDate;
                AnswerText = question.GetAnswerString();
            }
        }
        public Guid PublicKey { get; set; }
        public Guid GroupPublicKey { get; set; }
        public Guid? GroupPropagationPublicKey { get; set; }
        public Guid ScreenPublicKey { get; set; }

        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string AnswerText { get; set; }
     //   public object AnswerValue { get; set; }
    }
}