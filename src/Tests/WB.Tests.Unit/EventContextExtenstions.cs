using System.Linq;
using Ncqrs.Spec;

namespace WB.Tests.Unit
{
    internal static class EventContextExtenstions
    {
        public static TEvent GetSingleEvent<TEvent>(this EventContext eventContext)
        {
            return (TEvent) eventContext.Events.Single(@event => @event.Payload is TEvent).Payload;
        }
    }
}