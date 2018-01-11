using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    [TestOf(typeof(InterviewPackagesService))]
    internal class InterviewPackagesServiceTestsContext
    {
        protected InterviewKey GeneratedInterviewKey = new InterviewKey(5533);
    }
}