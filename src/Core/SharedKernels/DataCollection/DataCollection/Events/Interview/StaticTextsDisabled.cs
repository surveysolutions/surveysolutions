using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDisabled : StaticTextsPassiveEvent
    {
        public StaticTextsDisabled(Identity[] staticTexts, DateTimeOffset originDate)
            : base(staticTexts, originDate) { }
    }
}
