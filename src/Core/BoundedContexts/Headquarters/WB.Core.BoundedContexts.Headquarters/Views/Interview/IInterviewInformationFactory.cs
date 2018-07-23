using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewInformationFactory
    {
        IEnumerable<InterviewInformation> GetInProgressInterviewsForInterviewer(Guid interviewerId);
        IEnumerable<InterviewInformation> GetInProgressInterviewsForSupervisor(Guid supervisorId);
        IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds);
        [Obsolete("Since 18.08 KP-11379")]
        InterviewSynchronizationDto GetInProgressInterviewDetails(Guid interviewId);
    }
}
