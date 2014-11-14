using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
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

        internal class TestableFunctionalEventHandlerWhichDeletesUpdatedView : AbstractFunctionalEventHandler<IReadSideRepositoryEntity>, IUpdateHandler<IReadSideRepositoryEntity, object>
        {
            public TestableFunctionalEventHandlerWhichDeletesUpdatedView(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readsideRepositoryWriter)
                : base(readsideRepositoryWriter) { }

            public int CountOfUpdates { get; private set; }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity currentState, IPublishedEvent<object> evnt)
            {
                CountOfUpdates++;
                return null;
            }
        }
    }
}
