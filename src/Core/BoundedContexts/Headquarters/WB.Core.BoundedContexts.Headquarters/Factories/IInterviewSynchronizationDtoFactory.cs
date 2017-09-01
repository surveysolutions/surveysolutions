using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    [Obsolete("Now we use StatefulInterview.GetSynchronizationDto()")]
    public interface IInterviewSynchronizationDtoFactory 
    {
        InterviewSynchronizationDto BuildFrom(InterviewData interview, Guid userId, InterviewStatus status, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime);
        InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime);
    }
}