using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;

namespace WB.Core.SharedKernels.Enumerator.Events
{
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
