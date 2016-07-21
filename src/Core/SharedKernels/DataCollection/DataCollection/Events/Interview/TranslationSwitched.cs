using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TranslationSwitched : InterviewActiveEvent
    {
        public TranslationSwitched(Guid? translationId, Guid userId)
            : base(userId)
        {
            this.TranslationId = translationId;
        }

        public Guid? TranslationId { get; }
    }
}