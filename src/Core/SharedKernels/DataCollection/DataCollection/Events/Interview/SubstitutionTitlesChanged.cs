using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SubstitutionTitlesChanged : QuestionsPassiveEvent
    {
        public Identity[] StaticTexts { get; }

        public SubstitutionTitlesChanged(Identity[] questions, Identity[] staticTexts)
            : base(questions)
        {
            this.StaticTexts = staticTexts?.ToArray() ?? new Identity[]{};
        }
    }
}