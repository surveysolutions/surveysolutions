using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewInformationFactory
    {
        List<InterviewInformation> GetInProgressInterviewsForInterviewer(Guid interviewerId);
        List<InterviewInformation> GetInProgressInterviewsForSupervisor(Guid supervisorId);
        IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds);
        [Obsolete("Since 18.08 KP-11379")]
        InterviewSynchronizationDto GetInProgressInterviewDetails(Guid interviewId);

        bool HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(Guid authorizedUserId);
        bool HasAnyInterviewsInProgressWithResolvedCommentsForSupervisor(Guid authorizedUserId);
    }
}
