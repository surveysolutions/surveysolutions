using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Events;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace CAPI.Androids.Core.Model.Tests
{
    [TestFixture]
    public class ChangeLogManipulatorTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void OpenDraftRecord_When_if_and_event_start_are_not_empty_Then_new_record_was_create_in_draft_storage()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);
            Guid eventSourceId = Guid.NewGuid();
            int start = 4;

            // act
            
            unitUnderTest.OpenDraftRecord(eventSourceId, start);

            // assert
            Assert.That(draftStorage.Count,Is.EqualTo( 1));
            Assert.That(draftStorage.First().Value.Start, Is.EqualTo(start));
        }

        [Test]
        public void OpenDraftRecord_When_if_and_event_draft_record_presents_Then_invalidoprerationexception()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);
            Guid eventSourceId = Guid.NewGuid();
            Guid recordId = Guid.NewGuid();
            draftStorage[recordId] = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, 1, null);
            int start = 4;

            // act

            Assert.Throws<InvalidOperationException>(() => unitUnderTest.OpenDraftRecord(eventSourceId, start));
        }

        [Test]
        public void CloseDraftRecord_When_record_is_exists_Then_record_is_closed_and_file_is_saved()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            Mock<IChangeLogStore> changeLogStoreMock=new Mock<IChangeLogStore>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorage(changeLogStoreMock.Object, draftStorage);
            Guid eventSourceId = Guid.NewGuid();
            Guid recordId = Guid.NewGuid();
            int start = 4;
            draftStorage[recordId] = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, start, null);
            int end = 6;

            // act
            unitUnderTest.CloseDraftRecord(eventSourceId, end);

            // assert
            Assert.That(draftStorage.Count, Is.EqualTo(1));
            Assert.That(draftStorage[recordId].Start, Is.EqualTo(start));
            Assert.That(draftStorage[recordId].End, Is.EqualTo(end));
            changeLogStoreMock.Verify(x => x.SaveChangeset(It.IsAny<AggregateRootEvent[]>(), recordId), Times.Once());
        }

        [Test]
        public void CloseDraftRecord_When_record_is_exists_and_start_is_more_than_end_Then_argumentexception()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);
            Guid eventSourceId = Guid.NewGuid();
            Guid recordId = Guid.NewGuid();
            int start = 4;
            draftStorage[recordId] = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, start, null);
            int end = 1;

            // act
            Assert.Throws<ArgumentException>(() => unitUnderTest.CloseDraftRecord(eventSourceId, end));
        }

        [Test]
        public void CloseDraftRecord_When_record_is_absent_Then_nothing()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);
            Guid eventSourceId = Guid.NewGuid();
        
            int end = 6;

            // act
            unitUnderTest.CloseDraftRecord(eventSourceId, end);

            // assert
            Assert.That(draftStorage.Count, Is.EqualTo(0));
        }

        [Test]
        public void ReopenDraftRecord_When_record_for_reopen_presents_Then_end_record_is_removed()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            Mock<IChangeLogStore> changeLogStoreMock = new Mock<IChangeLogStore>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorage(changeLogStoreMock.Object, draftStorage);
            Guid eventSourceId = Guid.NewGuid();
            Guid recordId = Guid.NewGuid();
            int start = 4;
            int end = 10;
            draftStorage[recordId] = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, start, end);
            

            // act
            unitUnderTest.ReopenDraftRecord(eventSourceId);

            // assert
            Assert.That(draftStorage[recordId].End, Is.EqualTo(null));
            changeLogStoreMock.Verify(x => x.DeleteDraftChangeSet(recordId), Times.Once());
        }

        [Test]
        public void ReopenDraftRecord_When_record_for_reopen_absent_Then_nothing()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);
            Guid eventSourceId = Guid.NewGuid();

            // act
            unitUnderTest.ReopenDraftRecord(eventSourceId);

            // assert
            Assert.That(draftStorage.Count, Is.EqualTo(0));
        }

        [Test]
        public void MarkDraftChangesetAsPublic_When_record_is_presented_Then_record_is_moved_from_draft_to_public_and_removed_from_file_storage()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            var publicStorage = new Dictionary<Guid, PublicChangeSetDTO>();
            Mock<IChangeLogStore> changeLogStoreMock = new Mock<IChangeLogStore>();
            ChangeLogManipulator unitUnderTest =
                CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorageAndPublic(
                    changeLogStoreMock.Object, publicStorage, draftStorage);

            Guid eventSourceId = Guid.NewGuid();
            Guid recordId = Guid.NewGuid();
            int start = 4;
            int end = 10;
            draftStorage[recordId] = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, start, end);

            // act
            unitUnderTest.CleanUpChangeLogByRecordId(recordId);

            // assert
            Assert.That(draftStorage.Count,Is.EqualTo(0));
            //Assert.That(publicStorage.Count,Is.EqualTo(1));
            //Assert.That(publicStorage[recordId].Id, Is.EqualTo(recordId.ToString()));
            //Assert.That(result, Is.EqualTo(eventSourceId));
            changeLogStoreMock.Verify(x => x.DeleteDraftChangeSet(recordId), Times.Once());

        }

/*        [Test]
        public void MarkDraftChangesetAsPublic_When_record_is_absent_Then_InvalidOperationException()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            ChangeLogManipulator unitUnderTest = CreateChangeLogManipulatorWithAccesibleDraftStorage(draftStorage);

            Guid recordId = Guid.NewGuid();

            // act
            Assert.Throws<InvalidOperationException>(
                  () => unitUnderTest.CleanUpChangeLog(recordId));

            // assert
            
            Assert.That(draftStorage.Count, Is.EqualTo(0));

        }*/

        [Test]
        public void CreatePublicRecord_When_parameter_are_correct_Then_public_record_is_saved()
        {
            // arrange
            var draftStorage = new Dictionary<Guid, DraftChangesetDTO>();
            var publicStorage = new Dictionary<Guid, PublicChangeSetDTO>();
            Mock<IChangeLogStore> changeLogStoreMock = new Mock<IChangeLogStore>();
            ChangeLogManipulator unitUnderTest =
                CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorageAndPublic(
                    changeLogStoreMock.Object, publicStorage, draftStorage);
            Guid recordId = Guid.NewGuid();

            // act
            unitUnderTest.CreatePublicRecord(recordId);

            // assert
            Assert.That(publicStorage.Count, Is.EqualTo(1));
            Assert.That(publicStorage[recordId].Id, Is.EqualTo(recordId.ToString()));

        }

        private ChangeLogManipulator CreateChangeLogManipulatorWithAccesibleDraftStorage(
            Dictionary<Guid, DraftChangesetDTO> storage)
        {
            return
                CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorage(
                    new Mock<IChangeLogStore>().Object, storage);

        }

        private ChangeLogManipulator CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorage(
            IChangeLogStore changelogStore, Dictionary<Guid, DraftChangesetDTO> storage)
        {
            return CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorageAndPublic(changelogStore, new Dictionary<Guid, PublicChangeSetDTO>(), storage);
        }

        private ChangeLogManipulator CreateChangeLogManipulatorWithAccesibleChangelogStorageAndDraftStorageAndPublic(
            IChangeLogStore changelogStore, Dictionary<Guid, PublicChangeSetDTO>publicstorage, Dictionary<Guid, DraftChangesetDTO> draftstorage)
        {
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.ReadFrom(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<long>()))
                      .Returns<Guid,long,long>((e,start,end) => new CommittedEventStream(e, new List<CommittedEvent>()));
            return new ChangeLogManipulator(new FilterableDenormalizerStorageStub<PublicChangeSetDTO>(publicstorage),
                                            new FilterableDenormalizerStorageStub<DraftChangesetDTO>(draftstorage),
                                            eventStore.Object, changelogStore);

        }
    }
}