using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    public class PublishedEvent<T> where T : IEvent
    {
        public Guid EventSourceId { get; set; }
        public T Event { get; set; }
    }
}
