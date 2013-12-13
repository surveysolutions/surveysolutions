﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionEnabled : QuestionPassiveEvent
    {
        public QuestionEnabled(Guid questionId, decimal[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}