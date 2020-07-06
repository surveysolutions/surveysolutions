using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentReassigned : AssignmentEvent
    {
        public Guid ResponsibleId { get; set; }
        public string Comment { get; set; } = String.Empty;
    }
}
