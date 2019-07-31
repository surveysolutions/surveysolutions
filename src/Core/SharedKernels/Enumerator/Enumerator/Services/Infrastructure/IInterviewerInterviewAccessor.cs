﻿using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IInterviewerInterviewAccessor
    {
        void RemoveInterview(Guid interviewId);
        InterviewPackageApiView GetInterviewEventsPackageOrNull(Guid interviewId);
        IReadOnlyCollection<CommittedEvent> GetPendingInteviewEvents(Guid interviewId);
        EventStreamSignatureTag GetInterviewEventStreamCheckData(Guid interviewId);

        void CheckAndProcessInterviewsWithoutViews();
    }
}
