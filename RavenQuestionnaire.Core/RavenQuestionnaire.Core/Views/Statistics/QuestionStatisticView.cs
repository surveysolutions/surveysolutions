using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents.Statistics;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class QuestionStatisticView
    {
        public QuestionStatisticView(QuestionStatisticDocument doc)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
            this.AnswerValue = new CompleteQuestionFactory().GetAnswerString(doc.QuestionType, doc.AnswerValue);
            this.AnswerDate = doc.AnswerDate;
            this.ApproximateTime = doc.ApproximateTime;
            this.AnswerText = doc.AnswerText;
        }

        public Guid PublicKey { get; set; }

        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public string AnswerValue { get; set; }
        public string AnswerText { get; set; }
    }
}
