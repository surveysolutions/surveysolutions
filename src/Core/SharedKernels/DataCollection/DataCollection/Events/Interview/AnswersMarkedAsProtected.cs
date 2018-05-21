using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersMarkedAsProtected : QuestionsPassiveEvent
    {
        public AnswersMarkedAsProtected(Identity[] questions) : base(questions)
        {
        }
    }
}