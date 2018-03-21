using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Memory.Implementation;


namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    internal class when_Clear_called : MemoryCachedReadSideStoreTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var readSideStorageMock=new Mock<IReadSideStorage<ReadSideRepositoryEntity>>();
            readSideRepositoryCleanerMock = readSideStorageMock.As<IReadSideRepositoryCleaner>();
            memoryCachedReadSideStorage = CreateMemoryCachedReadSideStore(readSideStorageMock.Object);
            BecauseOf();
        }
        public void BecauseOf() =>
            memoryCachedReadSideStorage.Clear();

        [NUnit.Framework.Test] public void should_call_clear_of_IReadSideStorage () =>
            readSideRepositoryCleanerMock.Verify(x => x.Clear(), Times.Once);
        
        private static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> memoryCachedReadSideStorage;
        private static Mock<IReadSideRepositoryCleaner> readSideRepositoryCleanerMock;
    }
}
