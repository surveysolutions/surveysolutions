using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_view_is_deleted_by_update_handler : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            readSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            readSideRepositoryWriterMock.Setup(x => x.GetById(eventSourceId.FormatGuid())).Returns(CreateReadSideRepositoryEntity());
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWhichDeletesUpdatedView(readSideRepositoryWriterMock.Object);
        };

        Because of = () => testableFunctionalEventHandler.Handle(new[] { CreatePublishableEvent() }, eventSourceId);

        It should_readSideRepositoryWriters_method_Remove_called_only_once_at_firts_read = () =>
            readSideRepositoryWriterMock.Verify(x => x.Remove(eventSourceId.FormatGuid()), Times.Once());

        It should_count_of_updates_be_equal_to_1 = () =>
           testableFunctionalEventHandler.CountOfUpdates.ShouldEqual(1);

        private static TestableFunctionalEventHandlerWhichDeletesUpdatedView testableFunctionalEventHandler;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> readSideRepositoryWriterMock;
        private static Guid eventSourceId;

        internal class TestableFunctionalEventHandlerWhichDeletesUpdatedView : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>, IUpdateHandler<IReadSideRepositoryEntity, object>
        {
            public TestableFunctionalEventHandlerWhichDeletesUpdatedView(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideStorage)
                : base(readSideStorage) { }

            public int CountOfUpdates { get; private set; }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity currentState, IPublishedEvent<object> evnt)
            {
                this.CountOfUpdates++;
                return null;
            }
        }
    }
}
