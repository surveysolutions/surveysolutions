using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    [Subject(typeof(SimpleSynchronizationDataStorage))]
    internal class SimpleSynchronizationDataStorageTestContext
    {
        protected static SimpleSynchronizationDataStorage GetSimpleSynchronizationDataStorage(
            IQueryableReadSideRepositoryReader<UserDocument> userStorage = null,
            IChunkWriter chunkStorageWriter = null, IChunkReader chunkStorageReader = null,
            IMetaInfoBuilder metaBuilder = null, IArchiveUtils archiver = null)
        {
            return new SimpleSynchronizationDataStorage(userStorage ??  Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                chunkStorageWriter ?? Mock.Of<IChunkWriter>(),
                chunkStorageReader ?? Mock.Of<IChunkReader>(),
                metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                archiver ?? Mock.Of<IArchiveUtils>());
        }


        protected static Mock<IChunkWriter> CreateIChunkWriter()
        {
            var result = new Mock<IChunkWriter>();
            return result;
        }
    }
}
