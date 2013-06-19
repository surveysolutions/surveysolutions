using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Integration
{
    [TestFixture]
    public class FileChunkStorageTests
    {
        private const string FolderPath = ".";
        private const string FolderName= "SyncPath";

        [SetUp]
        public void SetUp()
        {
            var dirPath = Path.Combine(FolderPath, FolderName);
            if (Directory.Exists(dirPath))
                Directory.Delete(Path.Combine(FolderPath, FolderName), true);
        }

        [Test]
        public void ctor_When_chunks_are_presented_in_directory_Then_new_chunck_is_created_with_sequence_after_stored()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid supervisorId = Guid.NewGuid();
            var someContent = "some content";
            var dirPath = Path.Combine(FolderPath, FolderName, supervisorId.ToString());
            Directory.CreateDirectory(dirPath);

            int count = 5;
            for (int i = 1; i <= count; i++)
            {
                File.WriteAllText(Path.Combine(dirPath, CreateSyncFileName(i,Guid.NewGuid())), someContent);
            }

            FileChunkStorage target = CreateFileChunkStorage(supervisorId);

            // act
            target.StoreChunk(chunkId, someContent);

            // assert
            Assert.IsTrue(File.Exists(Path.Combine(dirPath, CreateSyncFileName(count + 1, chunkId))));

        }

        private string CreateSyncFileName(int i, Guid id)
        {
            return string.Format("{0}-{1}.sync", i, id);
        }

        [Test]
        public void StoreChunk_When_content_is_not_empty_Then_content_is_Saved()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            var someContent = "some content";
            FileChunkStorage target = CreateFileChunkStorage();

            // act
            target.StoreChunk(chunkId,someContent);

            // assert
            var storedChunck = target.ReadChunk(chunkId);
            Assert.That(storedChunck,Is.EqualTo(someContent));

        }

        [Test]
        public void StoreChunk_When_chunk_with_same_guid_is_present_Then_cunk_is_Stored_with_next_sequence_previous_is_deleted()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            Guid supervisorId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            FileChunkStorage target = CreateFileChunkStorage(supervisorId);
            target.StoreChunk(chunkId,someContent1);

            // act

            target.StoreChunk(chunkId, someContent2);

            // assert
            var storedChunck = target.ReadChunk(chunkId);
            Assert.That(storedChunck, Is.EqualTo(someContent2));
            Assert.That(Directory.GetFiles(Path.Combine(FolderPath, FolderName,supervisorId.ToString())).Count(), Is.EqualTo(1));
        }

        [Test]
        public void ReadChunk_When_cunck_is_absent_Then_ArgumentException_thrown()
        {
            // arrange
            Guid chunkId = Guid.NewGuid();
            FileChunkStorage target = CreateFileChunkStorage();

            // act and assert
            Assert.Throws<ArgumentException>(() => target.ReadChunk(chunkId));
        }

        [Test]
        public void ReadChunk_When_2_chunk_with_same_guid_is_presented_Then_content_Returned_by_latest_chunk()
        {
            // arrange

            Guid chunkId = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";
            FileChunkStorage target = CreateFileChunkStorage();
            target.StoreChunk(chunkId, someContent1);
            target.StoreChunk(chunkId, someContent2);

            // act

            var storedChunck = target.ReadChunk(chunkId);

            // assert
          
            Assert.That(storedChunck, Is.EqualTo(someContent2));
        }

        [Test]
        public void GetChunksCreatedAfter_When_squence_is_0_Then_all_roots_are_returned()
        {
            // arrange
            Guid chunkId1 = Guid.NewGuid();
            Guid chunkId2 = Guid.NewGuid();
            var someContent1 = "some content1";
            var someContent2 = "some content2";

            FileChunkStorage target = CreateFileChunkStorage();

            target.StoreChunk(chunkId1, someContent1);
            target.StoreChunk(chunkId2, someContent2);

            // act
            var result = target.GetChunksCreatedAfter(0);

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
            var someContent1 = "some content1";
            var someContent2 = "some content2";

            FileChunkStorage target = CreateFileChunkStorage();

            target.StoreChunk(chunkId, someContent1);
            target.StoreChunk(chunkId, someContent2);

            // act
            var result = target.GetChunksCreatedAfter(0);

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
            var someContent = "some content";

            FileChunkStorage target = CreateFileChunkStorage();

            for (int i = 0; i < ind; i++)
            {
                target.StoreChunk(Guid.NewGuid(), someContent);
            }

            target.StoreChunk(chunkId, someContent);
            // act
            var result = target.GetChunksCreatedAfter(5);

            // assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.IsTrue(result.Contains(chunkId));
        }

        private FileChunkStorage CreateFileChunkStorage()
        {
            return CreateFileChunkStorage( Guid.NewGuid());
        }

        private FileChunkStorage CreateFileChunkStorage(Guid supervisorId)
        {
            return new FileChunkStorage(Path.Combine(FolderPath, FolderName), supervisorId);
        }
    }
}
