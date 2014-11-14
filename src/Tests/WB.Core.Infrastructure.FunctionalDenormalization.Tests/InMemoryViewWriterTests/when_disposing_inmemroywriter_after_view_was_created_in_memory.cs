using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.InMemoryViewWriterTests
{
    internal class when_disposing_inmemroywriter_after_view_was_created_in_memory : InMemoryViewWriterTestContext
    {
        Establish context = () =>
        {
            viewId = Guid.NewGuid();
            baseReadSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            baseReadSideRepositoryWriterMock.Setup(x => x.GetById(viewId.FormatGuid())).Returns(null as IReadSideRepositoryEntity);

            view = Mock.Of<IReadSideRepositoryEntity>();
            inMemoryViewWriter = CreateInMemoryViewWriter(baseReadSideRepositoryWriterMock.Object, viewId);
            inMemoryViewWriter.Store(view, viewId);
        };

        Because of = () => inMemoryViewWriter.Dispose();

        It should_call_store_method_of_base_readSideRepositoryWriter = () =>
            baseReadSideRepositoryWriterMock.Verify(x => x.Store(view, viewId.FormatGuid()), Times.Once());

        private static InMemoryViewWriter<IReadSideRepositoryEntity> inMemoryViewWriter;
        private static Guid viewId;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> baseReadSideRepositoryWriterMock;
        private static IReadSideRepositoryEntity view;
    }
}
