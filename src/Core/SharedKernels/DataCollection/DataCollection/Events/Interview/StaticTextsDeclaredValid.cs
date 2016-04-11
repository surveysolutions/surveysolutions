using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredValid : StaticTextsPassiveEvent
    {
        public StaticTextsDeclaredValid(Identity[] questions)
            : base(questions) {}
    }
}