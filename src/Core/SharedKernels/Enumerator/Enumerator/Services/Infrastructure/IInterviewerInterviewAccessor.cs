using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IInterviewerInterviewAccessor
    {
        void RemoveInterview(Guid interviewId);
        InterviewPackageApiView GetInteviewEventsPackageOrNull(Guid interviewId);
        IReadOnlyCollection<CommittedEvent> GetPendingInteviewEvents(Guid interviewId);
    }
}
