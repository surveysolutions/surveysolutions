using WB.Services.Export.Events.Interview.Base;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview
{
    public class VariablesEnabled : InterviewPassiveEvent
    {
        public Identity[] Variables { get; set; } = new Identity[0];

    }
}
