using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredValid : QuestionsPassiveEvent
    {
        public AnswersDeclaredValid(Dtos.Identity[] questions)
            : base(questions) {}
    }
}