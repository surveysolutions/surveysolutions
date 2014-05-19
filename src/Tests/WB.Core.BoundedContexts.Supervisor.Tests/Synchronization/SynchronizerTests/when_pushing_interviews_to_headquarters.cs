using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.SynchronizerTests
{
    internal class when_pushing_interviews_to_headquarters : SynchronizerTestsContext
    {
        Establish context = () =>
        {
            synchronizer = Create.Synchronizer(interviewsSynchronizerMock.Object);
        };

        Because of = () =>
            synchronizer.Push(userId);

        It should_push_via_interview_synchronizer_using_supplied_user_id = () =>
            interviewsSynchronizerMock.Verify(interviewsSynchronizer => interviewsSynchronizer.Push(userId), Times.Once);

        private static Mock<IInterviewsSynchronizer> interviewsSynchronizerMock = new Mock<IInterviewsSynchronizer>();
        private static Synchronizer synchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}