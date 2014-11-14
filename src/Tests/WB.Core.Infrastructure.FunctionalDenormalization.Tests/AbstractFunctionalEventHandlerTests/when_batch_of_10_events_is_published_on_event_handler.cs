﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    internal class when_batch_of_10_events_is_published_on_event_handler : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            readSideRepositoryWriterMock=new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            readSideRepositoryWriterMock.Setup(x => x.GetById(eventSourceId.FormatGuid())).Returns(CreateReadSideRepositoryEntity());
            testableFunctionalEventHandler = CreateAbstractFunctionalEventHandler(readSideRepositoryWriterMock.Object);
        };

        Because of = () => testableFunctionalEventHandler.Handle(CreatePublishableEvents(10), eventSourceId);

        It should_readSideRepositoryWriters_method_GetById_called_only_once_at_firts_read = () =>
            readSideRepositoryWriterMock.Verify(x => x.GetById(eventSourceId.FormatGuid()), Times.Once());

        It should_readSideRepositoryWriters_method_Store_called_once = () =>
            readSideRepositoryWriterMock.Verify(x => x.Store(Moq.It.IsAny<IReadSideRepositoryEntity>(), eventSourceId.FormatGuid()), Times.Once());

        It should_count_of_updates_be_equal_to_10 = () =>
           testableFunctionalEventHandler.CountOfUpdates.ShouldEqual(10);

        private static TestableFunctionalEventHandler<IReadSideRepositoryEntity> testableFunctionalEventHandler;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> readSideRepositoryWriterMock;
        private static Guid eventSourceId;
    }
}
