using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredInvalid : QuestionsPassiveEvent
    {
        public AnswersDeclaredInvalid(Dtos.Identity[] questions)
            : base(questions) {}
    }
}