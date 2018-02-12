using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredPlausible : QuestionsPassiveEvent
    {
        public AnswersDeclaredPlausible(Identity[] questions)
            : base(questions) {}
    }
}