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
    internal class when_disposing_inmemroywriter_after_view_was_deleted_in_memory : InMemoryViewWriterTestContext
    {
        Establish context = () =>
        {
            viewId = Guid.NewGuid();
            baseReadSideRepositoryWriterMock=new Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>();
            baseReadSideRepositoryWriterMock.Setup(x => x.GetById(viewId.FormatGuid())).Returns(Mock.Of<IReadSideRepositoryEntity>());

            inMemoryViewWriter = CreateInMemoryViewWriter(baseReadSideRepositoryWriterMock.Object, viewId);
            inMemoryViewWriter.Remove(viewId);
        };

        Because of = () => inMemoryViewWriter.Dispose();

        It should_call_remove_method_of_base_readSideRepositoryWriter = () =>
            baseReadSideRepositoryWriterMock.Verify(x => x.Remove(viewId.FormatGuid()), Times.Once());

        private static InMemoryViewWriter<IReadSideRepositoryEntity> inMemoryViewWriter;
        private static Guid viewId;
        private static Mock<IReadSideRepositoryWriter<IReadSideRepositoryEntity>> baseReadSideRepositoryWriterMock;
    }
}
