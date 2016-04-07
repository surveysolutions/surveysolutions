using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class StaticTextsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] StaticTexts { get; private set; }

        protected StaticTextsPassiveEvent(Identity[] staticTexts)
        {
            this.StaticTexts = staticTexts.ToArray();
        }
    }
}