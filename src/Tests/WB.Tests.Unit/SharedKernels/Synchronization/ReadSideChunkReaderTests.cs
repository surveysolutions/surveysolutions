using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.Synchronization
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
            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent1, DateTime.Now, userId), chunkId);
            querableStorageMock.Store(new SynchronizationDelta(chunkId, someContent2, DateTime.Now, userId), chunkId);

            // act

            var storedChunck = target.ReadChunk(chunkId);

            // assert

            Assert.That(storedChunck.Content, Is.EqualTo(someContent2));
        }

        [Test]
        public void GetChunkMetaDataCreatedAfter_after_specific_date_with_one_userId_that_equals_null()
        {
            // arrange
            Guid chunkId1 = Guid.NewGuid();
            Guid chunkId2 = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            var target = CreateRavenChunkReader(querableStorageMock);
            querableStorageMock.Store(new SynchronizationDelta(chunkId1, someContent1, DateTime.Now, userId), chunkId1);
            querableStorageMock.Store(new SynchronizationDelta(chunkId2, someContent2, DateTime.Now, null), chunkId2);

            // act
            var storedChunck = target.GetChunkMetaDataCreatedAfter(DateTime.Now.AddDays(-500), new List<Guid> { userId });

            // assert
            Assert.That(storedChunck.ToList().Count, Is.EqualTo(2));
        }

        private ReadSideChunkReader CreateRavenChunkReader(IQueryableReadSideRepositoryWriter<SynchronizationDelta> writeStorage)
        {
            return new ReadSideChunkReader(writeStorage);
        }
    }
}
