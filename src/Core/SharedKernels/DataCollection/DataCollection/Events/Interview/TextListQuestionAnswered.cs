using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextListQuestionAnswered : QuestionAnswered
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public TextListQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, Tuple<decimal, string>[] answers)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answers = answers;
        }
    }
}