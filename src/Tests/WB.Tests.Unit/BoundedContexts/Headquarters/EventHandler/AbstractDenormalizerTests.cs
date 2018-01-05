using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.EventHandler
{
    [TestOf(typeof(AbstractDenormalizer<>))]
    public class AbstractDenormalizerTests
    {
        [Test]
        public void should_not_save_state_for_events_that_are_not_handled_by_denormalizer()
        {
            var interviewId = Id.gA;
            var storageMock = new Mock<IReadSideRepositoryWriter<TestState>>();
            var denormalizer = new TestableAbstractDenormalizer(storageMock.Object);

            denormalizer.Handle(Create.PublishedEvent.InterviewApproved(interviewId).ToEnumerable(), interviewId);

            storageMock.Verify(x => x.Store(It.IsAny<TestState>(), It.IsAny<string>()), Times.Never, "Event is not handled by denormalizer and results should not be stored");
        }
    }
}