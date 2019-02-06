namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class GroupsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Groups { get; set; }
        
    }
}
