using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredPlausible : StaticTextsPassiveEvent
    {
        public StaticTextsDeclaredPlausible(Identity[] staticTexts, DateTimeOffset originDate)
            : base(staticTexts, originDate) {}
    }
}
