using System;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Events.Assignment.Base
{
    public abstract class AssignmentEvent : IEvent
    {
        public Guid UserId { get; set; }
        public DateTimeOffset OriginDate { get; set; }
    }
}
