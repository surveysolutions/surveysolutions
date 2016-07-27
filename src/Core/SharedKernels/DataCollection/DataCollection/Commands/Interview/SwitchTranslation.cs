using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SwitchTranslation : InterviewCommand
    {
        public SwitchTranslation(Guid interviewId, string language, Guid userId)
            : base(interviewId, userId)
        {
            this.Language = language;
        }

        public string Language { get; }
    }
}