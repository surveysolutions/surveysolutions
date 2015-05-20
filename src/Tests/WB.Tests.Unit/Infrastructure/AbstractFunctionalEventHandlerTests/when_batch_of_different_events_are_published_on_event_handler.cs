using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_batch_of_different_events_are_published_on_event_handler : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            readSideRepositoryWriterMock=new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            readSideRepositoryWriterMock.Setup(x => x.GetById(eventSourceId.FormatGuid())).Returns(CreateReadSideRepositoryEntity());
            testableFunctionalEventHandler = CreateAbstractFunctionalEventHandler(readSideRepositoryWriterMock.Object);
        };

        Because of = () => testableFunctionalEventHandler.Handle(new[] { CreatePublishableEvent(), CreatePublishableEvent(), CreatePublishableEvent(), CreatePublishableEvent("test") }, eventSourceId);

        It should_readSideRepositoryWriters_method_GetById_called_only_once_at_firts_read = () =>
            readSideRepositoryWriterMock.Verify(x => x.GetById(eventSourceId.FormatGuid()), Times.Once());

        It should_readSideRepositoryWriters_method_Store_called_once = () =>
            readSideRepositoryWriterMock.Verify(x => x.Store(Moq.It.IsAny<IReadSideRepositoryEntity>(), eventSourceId.FormatGuid()), Times.Once());

        It should_count_of_updates_be_equal_to_3 = () =>
           testableFunctionalEventHandler.CountOfUpdates.ShouldEqual(3);

        private static TestableFunctionalEventHandler testableFunctionalEventHandler;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> readSideRepositoryWriterMock;
        private static Guid eventSourceId;
    }
}
