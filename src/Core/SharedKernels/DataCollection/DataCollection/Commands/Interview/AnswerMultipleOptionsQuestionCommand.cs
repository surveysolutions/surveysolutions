﻿using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerMultipleOptionsQuestionCommand : AnswerQuestionCommand
    {
        public decimal[] SelectedValues { get; private set; }

        public AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedValues)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedValues = selectedValues;
        }
    }
}