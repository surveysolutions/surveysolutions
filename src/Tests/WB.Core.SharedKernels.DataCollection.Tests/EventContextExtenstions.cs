using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;

namespace WB.Core.SharedKernels.DataCollection.Tests
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
    }
}