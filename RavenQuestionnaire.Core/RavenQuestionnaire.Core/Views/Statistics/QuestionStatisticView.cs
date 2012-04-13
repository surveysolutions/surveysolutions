using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents.Statistics;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class QuestionStatisticView
    {
        public QuestionStatisticView(QuestionStatisticDocument doc)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
            this.AnswerValue = doc.AnswerValue;
            this.AnswerDate = doc.AnswerDate;
            this.ApproximateTime = doc.ApproximateTime;
        }

        public Guid PublicKey { get; set; }

        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public object AnswerValue { get; set; }
    }
}
