using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentCompleted : AssignmentEvent
    {
        public string? Comment { get; set; }
    }
}
