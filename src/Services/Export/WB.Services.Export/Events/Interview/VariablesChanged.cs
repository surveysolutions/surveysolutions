using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Events.Interview.Dtos;

namespace WB.Services.Export.Events.Interview
{
    public class VariablesChanged: InterviewPassiveEvent
    {
        public ChangedVariable[] ChangedVariables { get; set; } = new ChangedVariable[0];
    }
}
