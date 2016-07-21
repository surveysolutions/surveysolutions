using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SwitchTranslation : InterviewCommand
    {
        public SwitchTranslation(Guid interviewId, Guid? translationId, Guid userId)
            : base(interviewId, userId)
        {
            this.TranslationId = translationId;
        }

        public Guid? TranslationId { get; }
    }
}