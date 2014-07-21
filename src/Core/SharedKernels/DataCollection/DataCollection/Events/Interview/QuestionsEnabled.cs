using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionsEnabled : QuestionsPassiveEvent
    {
        public QuestionsEnabled(Dtos.Identity[] questions)
            : base(questions) {}
    }
}