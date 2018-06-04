using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TranslationSwitched : InterviewActiveEvent
    {
        public TranslationSwitched(string language, Guid userId, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.Language = language;
        }

        public string Language { get; }
    }
}
