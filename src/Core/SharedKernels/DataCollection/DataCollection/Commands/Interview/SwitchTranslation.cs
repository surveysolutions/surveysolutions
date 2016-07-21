using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SwitchTranslation : InterviewCommand
    {
        public SwitchTranslation(Guid interviewId, Guid? tranlstionId, Guid userId)
            : base(interviewId, userId)
        {
            this.TranlstionId = tranlstionId;
        }

        public Guid? TranlstionId { get; }
    }
}