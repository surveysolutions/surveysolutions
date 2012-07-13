using System;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class QuestionStatisticView
    {
        public QuestionStatisticView(ICompleteQuestion doc, Guid gropPublicKey, Guid? gropPropagationPublicKey,
                                     Guid screenPublicKey)
        {
            PublicKey = doc.PublicKey;
            QuestionText = doc.QuestionText;
            //    this.AnswerValue = doc.GetAnswerString();
            //   this.ApproximateTime = doc.ApproximateTime;
            GroupPublicKey = gropPublicKey;
            ScreenPublicKey = screenPublicKey;
            GroupPropagationPublicKey = gropPropagationPublicKey;
            if (gropPropagationPublicKey.HasValue)
                IsQuestionFromPropGroup = true;
            if (doc.Answer != null)
            {
                //     AnswerValue = answer.AnswerText?? answer.AnswerValue;
                AnswerDate = doc.AnswerDate;
                AnswerValue = AnswerText = doc.GetAnswerString();
            }
        }

        public Guid PublicKey { get; set; }
        public Guid GroupPublicKey { get; set; }
        public Guid? GroupPropagationPublicKey { get; set; }
        public Guid ScreenPublicKey { get; set; }

        public bool IsQuestionFromPropGroup { get; set; }
        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public string AnswerValue { get; set; }
        public string AnswerText { get; set; }
    }
}