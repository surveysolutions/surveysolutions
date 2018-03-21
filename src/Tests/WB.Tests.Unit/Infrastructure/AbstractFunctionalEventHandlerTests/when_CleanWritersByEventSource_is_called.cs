using System;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called : AbstractFunctionalEventHandlerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventSourceId = Guid.NewGuid();
            readSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            readSideRepositoryWriterMock.Setup(x => x.GetById(eventSourceId.FormatGuid())).Returns(CreateReadSideRepositoryEntity());
            testableFunctionalEventHandler = CreateAbstractFunctionalEventHandler(readSideRepositoryWriterMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId);

        [NUnit.Framework.Test] public void should_call_Remove_by_eventSourceId_only_once_at_firts_read () =>
            readSideRepositoryWriterMock.Verify(x => x.Remove(eventSourceId.FormatGuid()), Times.Once());

        private static TestableFunctionalEventHandler testableFunctionalEventHandler;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> readSideRepositoryWriterMock;
        private static Guid eventSourceId;
    }
}
