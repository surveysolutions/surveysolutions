using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;

namespace WB.Tests.Integration
{
    internal static class EventContextExtensions
    {
        public static int Count<T>(this EventContext eventContext)
        {
            return eventContext.GetEvents<T>().Count();
        }

        public static T GetSingleEvent<T>(this EventContext eventContext)
        {
            return eventContext.GetEvents<T>().Single();
        }

        public static T GetSingleEventOrNull<T>(this EventContext eventContext)
        {
            return eventContext.GetEvents<T>().SingleOrDefault();
        }

        public static IEnumerable<T> GetEvents<T>(this EventContext eventContext)
        {
            return eventContext.Events.Where(e => e.Payload is T).Select(e => (T)e.Payload);
        }

        public static bool AnyEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            return condition == null 
                ? eventContext.Events.Any(@event => @event.Payload is TEvent) 
                : eventContext.Events.Any(@event => @event.Payload is TEvent && condition.Invoke((TEvent)@event.Payload));
        }
    }
}