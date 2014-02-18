﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.InMemoryViewWriterTests
{
    internal class when_disposing_inmemroywriter_after_view_was_updated_in_memory : InMemoryViewWriterTestContext
    {
        Establish context = () =>
        {
            viewId = Guid.NewGuid();
            oldView = Mock.Of<IReadSideRepositoryEntity>();
            baseReadSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();

            baseReadSideRepositoryWriterMock.Setup(x => x.GetById(viewId.FormatGuid())).Returns(oldView);

            newView = Mock.Of<IReadSideRepositoryEntity>();
            inMemoryViewWriter = CreateInMemoryViewWriter(baseReadSideRepositoryWriterMock.Object, viewId);
            inMemoryViewWriter.Store(newView, viewId);
        };

        Because of = () => inMemoryViewWriter.Dispose();

        It should_call_store_method_with_updated_view_of_base_readSideRepositoryWriter = () =>
            baseReadSideRepositoryWriterMock.Verify(x => x.Store(newView, viewId.FormatGuid()), Times.Once());

        private static InMemoryViewWriter<IReadSideRepositoryEntity> inMemoryViewWriter;
        private static Guid viewId;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> baseReadSideRepositoryWriterMock;
        private static IReadSideRepositoryEntity newView;
        private static IReadSideRepositoryEntity oldView;
    }
}
