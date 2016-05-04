using System.Collections.Generic;
using System.Linq;

namespace WB.Tests.Unit.Designer
{
    internal static class EventContextExtenstions
    {
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