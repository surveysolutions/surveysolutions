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

        public static IEnumerable<T> GetEvents<T>(this EventContext eventContext)
        {
            return eventContext.Events.Where(e => e.Payload is T).Select(e => (T)e.Payload);
        }
    }
}