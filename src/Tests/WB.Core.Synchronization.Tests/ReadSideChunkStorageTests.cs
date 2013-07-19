using System;
using System.Linq;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Tests
{
    [TestFixture]
    public class ReadSideChunkStorageTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void ctor_When_chunks_are_presented_in_storage_Then_new_chunck_is_created_with_sequence_after_stored()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent = "some content";

            int count = 5;
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();

            for (int i = 1; i <= count; i++)
            {
                var id = Guid.NewGuid();
                querableStorageMock.Store(new SynchronizationDelta(id, "t", i, Guid.NewGuid()), id);
            }



            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);

            // act
            target.StoreChunk(new SyncItem(){Id = chunkId,Content = someContent}, userId);

            // assert

            var result = querableStorageMock.GetById(chunkId);
            Assert.That((object) result.Sequence,Is.EqualTo(count+1));

        }

        [Test]
        public void StoreChunk_When_content_is_not_empty_Then_content_is_Saved()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent = "some content";
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);

            // act
            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent, IsCompressed = false }, userId);

            // assert
            var storedChunck = target.ReadChunk(chunkId);
            Assert.That(storedChunck.Content,Is.EqualTo(someContent));

        }

        [Test]
        public void StoreChunk_When_chunk_with_same_guid_is_present_Then_cunk_is_Stored_with_next_sequence_previous_is_deleted()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent1, IsCompressed = false }, userId);

            // act

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent2, IsCompressed = false }, userId);

            // assert
            var storedChunck = target.ReadChunk(chunkId);
            Assert.That(storedChunck.Content, Is.EqualTo(someContent2));
            Assert.That( querableStorageMock.Count(), Is.EqualTo(1));
        }
        
        [Test]
        public void ReadChunk_When_cunck_is_absent_Then_ArgumentException_thrown()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            ReadSideChunkStorage target = CreateRavenChunkStorage();

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
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);
            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent1, IsCompressed = false}, userId);
            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent2, IsCompressed = false }, userId);

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
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);

            target.StoreChunk(new SyncItem() { Id = chunkId1, Content = someContent1 }, userId);
            target.StoreChunk(new SyncItem() { Id = chunkId2, Content = someContent2 }, userId);

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
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent1 }, userId);
            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent2 }, userId);

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
            ReadSideChunkStorage target = CreateRavenChunkStorage(querableStorageMock, querableStorageMock);


            for (int i = 0; i < ind; i++)
            {
                target.StoreChunk(new SyncItem() { Id = Guid.NewGuid(), Content = someContent }, userId);
            }

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent }, userId);
            // act
            var result = target.GetChunksCreatedAfterForUsers(5, new Guid[] { userId });

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.IsTrue(result.Contains(chunkId));
        }

        private ReadSideChunkStorage CreateRavenChunkStorage()
        {
            var storageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            return CreateRavenChunkStorage(storageMock, storageMock);
        }

        private ReadSideChunkStorage CreateRavenChunkStorage(IReadSideRepositoryWriter<SynchronizationDelta> storage, IQueryableReadSideRepositoryReader<SynchronizationDelta> querableStorage)
        {
            return new ReadSideChunkStorage(storage, querableStorage);
        }
    }
}
