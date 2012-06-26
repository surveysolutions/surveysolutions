using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class QuestionStatisticView
    {
        public QuestionStatisticView(ICompleteQuestion doc, Guid gropPublicKey, Guid screenPublicKey)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
        //    this.AnswerValue = doc.GetAnswerString();
         //   this.ApproximateTime = doc.ApproximateTime;
            this.GroupPublicKey = gropPublicKey;
            this.ScreenPublicKey = screenPublicKey;
            if (doc.Answer != null)
            {
                //     AnswerValue = answer.AnswerText?? answer.AnswerValue;
                AnswerDate = doc.AnswerDate;
                this.AnswerValue = AnswerText = doc.GetAnswerString();
            }
        }

        public Guid PublicKey { get; set; }
        public Guid GroupPublicKey { get; set; }
        public Guid ScreenPublicKey { get; set; }

        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public string AnswerValue { get; set; }
        public string AnswerText { get; set; }
    }
}
