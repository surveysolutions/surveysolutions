using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Questions { get; set; } = new Identity[0];
    }
}
