using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class VariablesDisabled: InterviewPassiveEvent
    {
        public Identity[] Variables { get; set; }

    }
}
