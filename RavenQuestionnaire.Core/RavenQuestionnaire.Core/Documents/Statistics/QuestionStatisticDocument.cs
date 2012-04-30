using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.Extensions;

namespace RavenQuestionnaire.Core.Documents.Statistics
{
    public class QuestionStatisticDocument
    {
        public QuestionStatisticDocument()
        {
        }

        public QuestionStatisticDocument(ICompleteQuestion question )
        {
            this.PublicKey = question.PublicKey;
            this.QuestionText = question.QuestionText;
            this.AnswerValue = question.GetValue();
            this.AnswerDate = question.AnswerDate;
            this.AnswerText = question.QuestionText;
        }

        public Guid PublicKey { get; set; }
        
        public DateTime? AnswerDate { get; set; }
        public TimeSpan? ApproximateTime { get; set; }
        public string QuestionText { get; set; }
        public object AnswerText { get; set; }
        public object AnswerValue { get; set; }
    }
}
