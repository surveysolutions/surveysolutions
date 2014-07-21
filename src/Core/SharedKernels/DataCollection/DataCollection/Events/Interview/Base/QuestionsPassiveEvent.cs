using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionsPassiveEvent : InterviewPassiveEvent
    {
        public Dtos.Identity[] Questions { get; private set; }

        protected QuestionsPassiveEvent(Dtos.Identity[] questions)
        {
            this.Questions = questions.ToArray();
        }
    }
}