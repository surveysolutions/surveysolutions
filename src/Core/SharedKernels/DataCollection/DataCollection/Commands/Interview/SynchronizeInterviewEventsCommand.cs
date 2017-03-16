﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewEventsCommand : InterviewCommand
    {
        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }

        public IEvent[] SynchronizedEvents { get; set; }

        public InterviewStatus InterviewStatus { get;  set; }

        public bool CreatedOnClient { get; set; }

        public InterviewKey InterviewKey { get; }

        public SynchronizeInterviewEventsCommand(Guid interviewId, 
            Guid userId,
            Guid questionnaireId,
            long questionnaireVersion,
            IEvent[] synchronizedEvents,
            InterviewStatus interviewStatus,
            bool createdOnClient,
            InterviewKey interviewKey)
            : base(interviewId, userId)
        {
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            SynchronizedEvents = synchronizedEvents;
            InterviewStatus = interviewStatus;
            CreatedOnClient = createdOnClient;
            this.InterviewKey = interviewKey;
        }
    }
}
