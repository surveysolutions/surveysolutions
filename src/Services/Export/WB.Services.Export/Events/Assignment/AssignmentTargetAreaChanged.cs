using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentTargetAreaChanged : AssignmentEvent
    {
        public string? TargetArea { get; set; }
    }
}
