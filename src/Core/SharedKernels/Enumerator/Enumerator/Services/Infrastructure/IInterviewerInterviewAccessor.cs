using System;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IInterviewerInterviewAccessor
    {
        void RemoveInterview(Guid interviewId);
        InterviewPackageApiView GetInterviewEventsPackageOrNull(InterviewPackageContainer packageContainer);
        
        InterviewPackageContainer GetInterviewEventStreamContainer(Guid interviewId, bool needCompress,
            SyncInfoPackageResponse syncInfoPackageResponse);
        InterviewSyncInfoPackage GetInterviewSyncInfoPackage(Guid interviewId);

        void CheckAndProcessInterviewsToFixViews();

        void MarkEventsAsReceivedByHQ(Guid interviewId);
    }
}
