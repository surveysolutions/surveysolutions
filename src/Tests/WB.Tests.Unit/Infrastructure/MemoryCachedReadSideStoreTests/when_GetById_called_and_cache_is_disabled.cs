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
    internal class when_GetById_called_and_cache_is_disabled : MemoryCachedReadSideStoreTestContext
    {
        Establish context = () =>
        {
            readSideStorageMock = new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
        };
        Because of = () =>
            memoryCachedReadSideStorage.GetById(id);

        It should_call_GetById_of_IReadSideStorage = () =>
            readSideStorageMock.Verify(x => x.GetById(id), Times.Once);

        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideStorage<ReadSideRepositoryEntity>> readSideStorageMock;
        private static string id = "id_view";
    }
}
