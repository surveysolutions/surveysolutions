using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;

namespace WB.Core.SharedKernels.Enumerator.Events
{
    public class InterviewAnswersFromSnapshotRestored : InterviewActiveEvent
    {
        public InterviewAnswersFromSnapshotRestored(InterviewAnswerDto[] interviewData, Guid userId)
            : base(userId)
        {
            this.Answers = interviewData;
        }

        public InterviewAnswerDto[] Answers { get; private set; }
    }
}
