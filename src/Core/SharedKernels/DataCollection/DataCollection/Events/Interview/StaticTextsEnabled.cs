using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsEnabled : StaticTextsPassiveEvent
    {
        public StaticTextsEnabled(Identity[] staticTexts)
            : base(staticTexts) {}
    }
}