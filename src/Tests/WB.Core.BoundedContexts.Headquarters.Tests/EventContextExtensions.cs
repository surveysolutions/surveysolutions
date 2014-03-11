using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    internal static class EventContextExtensions
    {
        public static TEvent GetSingleEvent<TEvent>(this EventContext eventContext)
        {
            return (TEvent)eventContext.Events.Single(@event => @event.Payload is TEvent).Payload;
        }

        public static void ShouldContainEvents<TEvent>(this EventContext eventContext, int count)
        {
            eventContext.Events.Count(e => e.Payload is TEvent).ShouldEqual(count);
        }

        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            if (condition == null)
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent
                    && condition.Invoke((TEvent) @event.Payload));
            }
        }

        public static void ShouldNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            if (condition == null)
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent
                    && condition.Invoke((TEvent) @event.Payload));
            }
        }
    }
}