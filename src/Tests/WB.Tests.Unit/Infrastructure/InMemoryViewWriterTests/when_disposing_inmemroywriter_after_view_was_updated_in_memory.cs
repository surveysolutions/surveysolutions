using System;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.InMemoryViewWriterTests
{
    internal class when_disposing_inmemroywriter_after_view_was_updated_in_memory : InMemoryViewWriterTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            viewId = Guid.NewGuid();
            oldView = Mock.Of<IReadSideRepositoryEntity>();
            baseReadSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();

            baseReadSideRepositoryWriterMock.Setup(x => x.GetById(viewId.FormatGuid())).Returns(oldView);

            newView = Mock.Of<IReadSideRepositoryEntity>();
            inMemoryViewWriter = CreateInMemoryViewWriter(baseReadSideRepositoryWriterMock.Object, viewId);
            inMemoryViewWriter.Store(newView, viewId);
            BecauseOf();
        }

        public void BecauseOf() => inMemoryViewWriter.Dispose();

        [NUnit.Framework.Test] public void should_call_store_method_with_updated_view_of_base_readSideRepositoryWriter () =>
            baseReadSideRepositoryWriterMock.Verify(x => x.Store(newView, viewId.FormatGuid()), Times.Once());

        private static InMemoryViewWriter<IReadSideRepositoryEntity> inMemoryViewWriter;
        private static Guid viewId;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> baseReadSideRepositoryWriterMock;
        private static IReadSideRepositoryEntity newView;
        private static IReadSideRepositoryEntity oldView;
    }
}
