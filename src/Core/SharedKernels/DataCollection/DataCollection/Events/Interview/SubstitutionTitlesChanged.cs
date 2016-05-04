using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SubstitutionTitlesChanged : QuestionsPassiveEvent
    {
        public SubstitutionTitlesChanged(Identity[] questions)
            : base(questions)
        {
        }
    }
}