using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentWebModeChanged : AssignmentEvent
    {
        public bool WebMode { get; set; }
    }
}
