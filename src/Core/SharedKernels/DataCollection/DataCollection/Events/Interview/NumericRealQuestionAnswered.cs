﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class NumericRealQuestionAnswered : QuestionAnswered
    {
        public decimal Answer { get; private set; }

        public NumericRealQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, decimal answer, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.Answer = answer;
        }
    }
}
