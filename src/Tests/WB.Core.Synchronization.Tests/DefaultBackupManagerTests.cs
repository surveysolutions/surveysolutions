using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Synchronization.Implementation.ImportManager;

namespace WB.Core.Synchronization.Tests
{
    [TestFixture]
    public class DefaultBackupManagerTests
    {
        [Test]
        public void Backup_When_eventstore_is_null_Then_null_is_returned()
        {
            // arrange
            DefaultBackupManager target = CreateDefaultBackupManager();

            // act
            var result =target.Backup();

            // assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void Backup_When_EventStore_is_not_empty_Then_all_events_are_returned()
        {
            // arrange
            var eventStore=new Mock<IStreamableEventStore>();
            var eventList = new List<CommittedEvent[]>();
            const int ArCount = 2;
            const int EventsPerArCount = 3;
            for (int i = 0; i < ArCount; i++)
            {
                var eventsPerAr = new CommittedEvent[EventsPerArCount];
                var eventSourceId = Guid.NewGuid();
                for (int j = 0; j < EventsPerArCount; j++)
                {
                    eventsPerAr[j] = new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId,
                                                                       j + 1, DateTime.Now, new object(), new Version());
                }
                eventList.Add(eventsPerAr);
            }

            eventStore.Setup(x => x.GetAllEventsIncludingSnapshots(It.IsAny<int>())).Returns(eventList);
            NcqrsEnvironment.SetDefault(eventStore.Object as IEventStore);

            DefaultBackupManager target = CreateDefaultBackupManager();

            // act
            var result = target.Backup();

            // assert
            Assert.That(result.Events.Count(), Is.EqualTo(ArCount*EventsPerArCount));
        }

        [Test]
        public void Backup_When_EventStore_is_empty_Then_event_list_is_empty()
        {
            // arrange
            var eventStore = new Mock<IStreamableEventStore>();
            NcqrsEnvironment.SetDefault(eventStore.Object as IEventStore);
            DefaultBackupManager target = CreateDefaultBackupManager();

            // act
            var result = target.Backup();

            // assert
            Assert.That(result.Events.Count(), Is.EqualTo(0));
        }

        private DefaultBackupManager CreateDefaultBackupManager()
        {
            return new DefaultBackupManager();
        }
    }
}
