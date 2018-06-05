using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QRBarcodeQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public QRBarcodeQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, string answer, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.Answer = answer;
        }
    }
}
