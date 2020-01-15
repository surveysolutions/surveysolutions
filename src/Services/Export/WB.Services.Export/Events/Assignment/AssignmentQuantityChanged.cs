using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentQuantityChanged : AssignmentEvent
    {
        public int? Quantity { get; set; }
    }
}
