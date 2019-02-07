using System;
using System.Reflection;
using WB.Services.Export.Events;

namespace WB.Services.Export.Handlers
{
    public static class PublishedEventConstructor
    {
        public static object AsPublishedEvent(this Event ev)
        {
            var eventType = ev.Payload.GetType();
            var genericEventType = typeof(PublishedEvent<>).MakeGenericType(eventType);
            var ctor = genericEventType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(Event) }, null);

            ctor = ctor ?? throw new ArgumentException(
                       $"Cannot found a public constructor of PublishedEvent for event type: {eventType.Name}");

            return ctor.Invoke(new[] {(object) ev});
        }
    }
}
