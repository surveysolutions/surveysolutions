using Machine.Specifications;

using Moq;

using Raven.Client.Document;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    internal class when_created : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            registryMock = new Mock<IReadSideRepositoryWriterRegistry>();
        };

        Because of = () =>
            writer = new RavenReadSideRepositoryWriter<IReadSideRepositoryEntity>(
                new DocumentStore(), registryMock.Object);

        It should_register_itself_in_writer_registry = () =>
            registryMock.Verify(registry => registry.Register(writer), Times.Once());

        private static Mock<IReadSideRepositoryWriterRegistry> registryMock;
        private static RavenReadSideRepositoryWriter<IReadSideRepositoryEntity> writer;
    }
}