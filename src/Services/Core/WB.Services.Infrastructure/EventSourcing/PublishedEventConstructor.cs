using System;
using System.Diagnostics;
using System.Reflection;

namespace WB.Services.Infrastructure.EventSourcing
{
    public static class PublishedEventConstructor
    {
        [DebuggerStepThrough]
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
