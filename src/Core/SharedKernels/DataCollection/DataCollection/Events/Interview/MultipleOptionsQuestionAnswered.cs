﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedValues { get; private set; }

        public MultipleOptionsQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc, decimal[] selectedValues)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.SelectedValues = selectedValues;
        }
    }
}