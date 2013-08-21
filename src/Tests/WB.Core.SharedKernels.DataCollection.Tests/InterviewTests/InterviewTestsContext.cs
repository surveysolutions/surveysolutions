using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static T GetEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }
    }
}