using System;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.InMemoryViewWriterTests
{
    internal class when_disposing_inmemroywriter_after_view_was_created_in_memory : InMemoryViewWriterTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            viewId = Guid.NewGuid();
            baseReadSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            baseReadSideRepositoryWriterMock.Setup(x => x.GetById(viewId.FormatGuid())).Returns(null as IReadSideRepositoryEntity);

            view = Mock.Of<IReadSideRepositoryEntity>();
            inMemoryViewWriter = CreateInMemoryViewWriter(baseReadSideRepositoryWriterMock.Object, viewId);
            inMemoryViewWriter.Store(view, viewId);
            BecauseOf();
        }

        public void BecauseOf() => inMemoryViewWriter.Dispose();

        [NUnit.Framework.Test] public void should_call_store_method_of_base_readSideRepositoryWriter () =>
            baseReadSideRepositoryWriterMock.Verify(x => x.Store(view, viewId.FormatGuid()), Times.Once());

        private static InMemoryViewWriter<IReadSideRepositoryEntity> inMemoryViewWriter;
        private static Guid viewId;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> baseReadSideRepositoryWriterMock;
        private static IReadSideRepositoryEntity view;
    }
}
