using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredInvalid : QuestionsPassiveEvent
    {
        public AnswersDeclaredInvalid(Identity[] questions)
            : base(questions) {}
    }
}