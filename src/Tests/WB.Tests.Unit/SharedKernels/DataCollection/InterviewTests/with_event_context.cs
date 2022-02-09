using Ncqrs.Spec;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class with_event_context
    {
        [NUnit.Framework.OneTimeSetUp] public virtual void context () {
            eventContext = new EventContext();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        protected static EventContext eventContext;
    }
}
