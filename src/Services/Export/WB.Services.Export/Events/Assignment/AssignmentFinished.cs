using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentFinished : AssignmentEvent
    {
        public string? Comment { get; set; }
    }
}
