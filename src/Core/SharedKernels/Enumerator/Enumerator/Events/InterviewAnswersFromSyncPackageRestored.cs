using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;

namespace WB.Core.SharedKernels.Enumerator.Events
{
    [Obsolete("Since v6.0")]
    public class InterviewAnswersFromSyncPackageRestored : InterviewActiveEvent
    {
        public InterviewAnswersFromSyncPackageRestored(InterviewAnswerDto[] answers, Guid userId)
            : base(userId)
        {
            this.Answers = answers;
        }

        public InterviewAnswerDto[] Answers { get; private set; }
    }
}
