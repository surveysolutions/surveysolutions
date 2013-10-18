using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.DenormalizerStorage;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Tests
{
    [TestFixture]
    public class ReadSideChunkReaderTests
    {
        [Test]
        public void ReadChunk_When_cunck_is_absent_Then_ArgumentException_thrown()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            ReadSideChunkReader target = CreateRavenChunkReader(new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>());

            // act and assert
            Assert.Throws<ArgumentException>(() => target.ReadChunk(chunkId));
        }

        [Test]
        public void ReadChunk_When_2_chunk_with_same_guid_is_presented_Then_content_Returned_by_latest_chunk()
        {
            // arrange

            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkReader target = CreateRavenChunkReader(querableStorageMock);
            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent1, 1, userId), chunkId);
            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent2, 2, userId), chunkId);

            // act

            var storedChunck = target.ReadChunk(chunkId);

            // assert

            Assert.That(storedChunck.Content, Is.EqualTo(someContent2));
        }
        private ReadSideChunkReader CreateRavenChunkReader(IQueryableReadSideRepositoryWriter<SynchronizationDelta> writeStorage)
        {
            return new ReadSideChunkReader(writeStorage);
        }

    }
}
