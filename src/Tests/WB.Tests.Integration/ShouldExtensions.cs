using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;

namespace WB.Tests.Integration
{
    internal static class ShouldExtensions
    {
        public static void ShouldContainEvent<TEvent>(this IEnumerable<IEvent> events, Func<TEvent, bool> condition = null)
            where TEvent: IEvent
        {
            if (condition == null)
            {
                events.Should().Contain(@event
                    => @event is TEvent);
            }
            else
            {
                events.Should().Contain(@event
                    => @event is TEvent
                    && condition.Invoke((TEvent) @event));
            }
        }

        public static void ShouldNotContainEvent<TEvent>(this IEnumerable<IEvent> events, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            if (condition == null)
            {
                events.Should().NotContain(@event
                    => @event is TEvent);
            }
            else
            {
                events.Should().NotContain(@event
                    => @event is TEvent && condition.Invoke((TEvent) @event));
            }
        }
        
        public static void ShouldContainEvents<TEvent>(this IEnumerable<IEvent> events, int count) where TEvent : IEvent
        {
            events.Count(e => e is TEvent).Should().Be(count);
        }

        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null) where TEvent : IEvent
        {
            eventContext.Events.Select(e => e.Payload).ShouldContainEvent(condition);
        }

        public static void ShouldNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null) where TEvent : IEvent
        {
            eventContext.Events.Select(e => e.Payload).ShouldNotContainEvent(condition);
        }
        
        public static void ShouldContainEvents<TEvent>(this EventContext eventContext, int count) where TEvent : IEvent
        {
            eventContext.Events.Count(e => e.Payload is TEvent).Should().Be(count);
        }
    }
}
