﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRestarted : InterviewActiveEvent
    {
        public InterviewRestarted(Guid userId, DateTime restartTime)
            : base(userId)
        {
            this.RestartTime = restartTime;
        }

        public DateTime RestartTime { get; private set; }
    }
}