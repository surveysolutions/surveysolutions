using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class StaticTextsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] StaticTexts { get; set; } = new Identity[0];
    }
}
