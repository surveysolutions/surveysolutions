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

        [Test]
        public void GetChunksCreatedAfter_When_squence_is_0_Then_all_roots_are_returned()
        {
            // arrange
            Guid chunkId1 = Guid.NewGuid();
            Guid chunkId2 = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";

            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkReader target = CreateRavenChunkReader(querableStorageMock);

            querableStorageMock.Store(new SynchronizationDelta(chunkId1, someContent1, 1, userId), chunkId1);
            querableStorageMock.Store(new SynchronizationDelta(chunkId2, someContent2, 2, userId), chunkId2);

            // act
            var result = target.GetChunksCreatedAfterForUsers(0, new Guid[] { userId });

            // assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.IsTrue(result.Contains(chunkId1));
            Assert.IsTrue(result.Contains(chunkId2));
        }


        [Test]
        public void GetChunksCreatedAfter_When_squence_is_0_and_cunck_is_dublicated_buy_with_different_sequence_Then_cunckId_is_distincted()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";

            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkReader target = CreateRavenChunkReader(querableStorageMock);

            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent1, 1, userId), userId);
            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent2, 2, userId), userId);

            // act
            var result = target.GetChunksCreatedAfterForUsers(0, new Guid[] { userId });

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.IsTrue(result.Contains(chunkId));
        }

        [Test]
        public void GetChunksCreatedAfter_When_squence_is_5_Then_all_roots_are_returned_which_Was_changed_after_5()
        {
            // arrange
            int ind = 5;
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent = "some content";

            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkReader target = CreateRavenChunkReader(querableStorageMock);


            for (int i = 0; i < ind; i++)
            {
                var chunckTempId = Guid.NewGuid();
                querableStorageMock.Store(new SynchronizationDelta(chunckTempId, someContent, i + 1, userId), chunckTempId);
            }

            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent, ind + 1, userId), chunkId);
            // act
            var result = target.GetChunksCreatedAfterForUsers(5, new Guid[] { userId });

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.IsTrue(result.Contains(chunkId));
        }
        private ReadSideChunkReader CreateRavenChunkReader(IQueryableReadSideRepositoryReader<SynchronizationDelta> writeStorage)
        {
            return new ReadSideChunkReader(writeStorage);
        }

    }
}
