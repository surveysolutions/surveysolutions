using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsEnabled : StaticTextsPassiveEvent
    {
        public StaticTextsEnabled(Identity[] staticTexts, DateTimeOffset originDate)
            : base(staticTexts, originDate) {}
    }
}
