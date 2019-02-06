namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class StaticTextsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] StaticTexts { get; set; }
    }
}
