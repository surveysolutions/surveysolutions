using System;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_view_is_deleted_by_update_handler : AbstractFunctionalEventHandlerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventSourceId = Guid.NewGuid();
            readSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            readSideRepositoryWriterMock.Setup(x => x.GetById(eventSourceId.FormatGuid())).Returns(CreateReadSideRepositoryEntity());
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWhichDeletesUpdatedView(readSideRepositoryWriterMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => testableFunctionalEventHandler.Handle(new[] { CreatePublishableEvent() }, eventSourceId);

        [NUnit.Framework.Test] public void should_readSideRepositoryWriters_method_Remove_called_only_once_at_firts_read () =>
            readSideRepositoryWriterMock.Verify(x => x.Remove(eventSourceId.FormatGuid()), Times.Once());

        [NUnit.Framework.Test] public void should_count_of_updates_be_equal_to_1 () =>
           testableFunctionalEventHandler.CountOfUpdates.Should().Be(1);

        private static TestableFunctionalEventHandlerWhichDeletesUpdatedView testableFunctionalEventHandler;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> readSideRepositoryWriterMock;
        private static Guid eventSourceId;

        public class TestableFunctionalEventHandlerWhichDeletesUpdatedView : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>, IUpdateHandler<IReadSideRepositoryEntity, TestableFunctionalEvent>
        {
            public TestableFunctionalEventHandlerWhichDeletesUpdatedView(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideStorage)
                : base(readSideStorage) { }

            public int CountOfUpdates { get; private set; }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity state, IPublishedEvent<TestableFunctionalEvent> @event)
            {
                this.CountOfUpdates++;
                return null;
            }
        }
    }
}
