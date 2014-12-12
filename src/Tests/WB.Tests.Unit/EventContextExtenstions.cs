using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;

namespace WB.Tests.Unit
{
    internal static class EventContextExtenstions
    {
        public static T GetEvent<T>(this EventContext eventContext)
        {
            return eventContext.GetEvents<T>().Single();
        }

        public static IEnumerable<T> GetEvents<T>(this EventContext eventContext)
        {
            return eventContext.Events.Where(e => e.Payload is T).Select(e => (T)e.Payload);
        }

        public static TEvent GetSingleEvent<TEvent>(this EventContext eventContext)
        {
            return (TEvent) eventContext.Events.Single(@event => @event.Payload is TEvent).Payload;
        }
    }
}