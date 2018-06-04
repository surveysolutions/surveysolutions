﻿using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewOpenedBySupervisor : InterviewActiveEvent
    {
        public InterviewOpenedBySupervisor(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
            //check out why is it local but not UTC
            LocalTime = originDate.LocalDateTime;
        }
        public DateTime? LocalTime { get; set; }
    }
}
