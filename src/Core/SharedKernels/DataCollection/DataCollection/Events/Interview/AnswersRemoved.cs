using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        public AnswersRemoved(Identity[] questions)
            : base(questions) {}
    }
}