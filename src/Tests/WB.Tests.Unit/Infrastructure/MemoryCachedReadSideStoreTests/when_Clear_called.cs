﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_Clear_called : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            var readSideStorageMock=new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            readSideRepositoryCleanerMock = readSideStorageMock.As<IReadSideRepositoryCleaner>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
        };
        Because of = () =>
            memoryCachedReadSideStorage.Clear();

        It should_call_clear_of_IReadSideStorage = () =>
            readSideRepositoryCleanerMock.Verify(x => x.Clear(), Times.Once);
        
        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideRepositoryCleaner> readSideRepositoryCleanerMock;
    }
}
