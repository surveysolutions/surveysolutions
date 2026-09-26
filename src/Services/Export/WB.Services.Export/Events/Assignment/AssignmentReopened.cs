using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentReopened : AssignmentEvent
    {
        public string? Comment { get; set; }
    }
}
