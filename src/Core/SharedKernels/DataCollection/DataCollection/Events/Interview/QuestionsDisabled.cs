﻿using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionsDisabled : QuestionsPassiveEvent
    {
        public QuestionsDisabled(Identity[] questions)
            : base(questions) {}
    }
}