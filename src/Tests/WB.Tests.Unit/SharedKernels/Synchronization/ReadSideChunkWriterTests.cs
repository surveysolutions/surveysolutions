using System;
using System.Linq;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.Synchronization
{
    [TestFixture]
    public class ReadSideChunkWriterTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void StoreChunk_When_content_is_not_empty_Then_content_is_Saved()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent = "some content";
            IQueryableReadSideRepositoryWriter<SynchronizationDelta> querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkWriter target = CreateRavenChunkWriter(querableStorageMock);

            // act
            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent, IsCompressed = false }, userId, DateTime.Now);

            // assert
            var storedChunck = querableStorageMock.GetById(chunkId);
            Assert.That(storedChunck.Content,Is.EqualTo(someContent));

        }

        [Test]
        public void StoreChunk_When_chunk_with_same_guid_is_present_Then_chunk_is_Stored_with_next_sequence_previous_is_deleted()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            var querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDelta>();
            ReadSideChunkWriter target = CreateRavenChunkWriter(querableStorageMock);

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent1, IsCompressed = false }, userId, DateTime.Now);

            // act

            target.StoreChunk(new SyncItem() { Id = chunkId, Content = someContent2, IsCompressed = false }, userId, DateTime.Now);

            // assert
            var storedChunck = ((IQueryableReadSideRepositoryWriter<SynchronizationDelta>) querableStorageMock).GetById(chunkId);
            Assert.That(storedChunck.Content, Is.EqualTo(someContent2));
            Assert.That( querableStorageMock.Count(), Is.EqualTo(1));
        }
        

        private ReadSideChunkWriter CreateRavenChunkWriter(IQueryableReadSideRepositoryWriter<SynchronizationDelta> writeStorage)
        {
            return new ReadSideChunkWriter(writeStorage);
        }

    }
}
