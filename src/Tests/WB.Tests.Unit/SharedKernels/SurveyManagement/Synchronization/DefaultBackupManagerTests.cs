using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Tests.Unit.Infrastructure.LiteEventBusTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Synchronization
{
    [TestFixture]
    internal class DefaultBackupManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void Backup_When_eventstore_is_not_streamable_Then_null_is_returned()
        {
            // arrange
            var eventStore = new Mock<IEventStore>();
            DefaultBackupManager target = this.CreateDefaultBackupManager(eventStore.Object);

            // act
            var result = target.Backup();

            // assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void Backup_When_EventStore_is_not_empty_Then_all_events_are_returned()
        {
            // arrange
            var eventStore=new Mock<IStreamableEventStore>();
            var eventList = new List<CommittedEvent>();
            const int ArCount = 2;
            const int EventsPerArCount = 3;
            for (int i = 0; i < ArCount; i++)
            {
                var eventsPerAr = new CommittedEvent[EventsPerArCount];
                var eventSourceId = Guid.NewGuid();
                for (int j = 0; j < EventsPerArCount; j++)
                {
                    eventsPerAr[j] = new CommittedEvent(Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId,
                                                                       j + 1, DateTime.Now, 0, Mock.Of<ILiteEvent>());
                }
                eventList.AddRange(eventsPerAr);
            }

            eventStore.Setup(x => x.GetAllEvents()).Returns(eventList);

            DefaultBackupManager target = this.CreateDefaultBackupManager(eventStore.Object);

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
            DefaultBackupManager target = this.CreateDefaultBackupManager(eventStore.Object);

            // act
            var result = target.Backup();

            // assert
            Assert.That(result.Events.Count(), Is.EqualTo(0));
        }

        private DefaultBackupManager CreateDefaultBackupManager(IEventStore eventStore)
        {
            return new DefaultBackupManager(eventStore);
        }
    }
}
