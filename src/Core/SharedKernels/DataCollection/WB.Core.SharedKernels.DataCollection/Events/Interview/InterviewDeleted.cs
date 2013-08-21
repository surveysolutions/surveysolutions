﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewDeleted : InterviewActiveEvent
    {
        public InterviewDeleted(Guid userId)
            : base(userId) {}
    }
}