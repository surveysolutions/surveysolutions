using Machine.Specifications;
using Ncqrs.Spec;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class with_event_context
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        protected static EventContext eventContext;
    }
}