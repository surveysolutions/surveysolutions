using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredValid : QuestionsPassiveEvent
    {
        public AnswersDeclaredValid(Identity[] questions)
            : base(questions) {}
    }
}