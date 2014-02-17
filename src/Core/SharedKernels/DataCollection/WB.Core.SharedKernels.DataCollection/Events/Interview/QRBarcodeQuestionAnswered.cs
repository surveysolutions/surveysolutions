using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QRBarcodeQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public QRBarcodeQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, string answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}