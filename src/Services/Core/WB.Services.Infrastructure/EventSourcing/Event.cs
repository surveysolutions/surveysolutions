using System;
using System.Reflection;
using Newtonsoft.Json;
using WB.Services.Infrastructure.EventSourcing.Json;

namespace WB.Services.Infrastructure.EventSourcing
{
    [JsonConverter(typeof(FeedEventConverter))]
    public class Event
    {
        public string EventTypeName { get; set; } = String.Empty;

        public int Sequence { get; set; }

        public Guid EventSourceId { get; set; }

        public long GlobalSequence { get; set; }

        public IEvent? Payload { get; set; }
        public DateTime EventTimeStamp { get; set; }

        private object? publishedEvent;
        public object AsPublishedEvent()
        {
            if (publishedEvent != null) return publishedEvent;

            var ev = this;

            if(ev.Payload == null)
                throw new ArgumentException(
                    $"Cannot convert to PublishedEvent. Payload is null");

            var eventType = ev.Payload.GetType();
            var genericEventType = typeof(PublishedEvent<>).MakeGenericType(eventType);
            var ctor = genericEventType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(Event) }, null);

            ctor = ctor ?? throw new ArgumentException(
                       $"Cannot found a public constructor of PublishedEvent for event type: {eventType.Name}");

            publishedEvent = ctor.Invoke(new[] { (object)ev });

            return publishedEvent;
        }
    }
}
