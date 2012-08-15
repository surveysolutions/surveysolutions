using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Tests.Events
{
    [TestFixture]
    public class EventSyncExtensionsTests
    {
        [Test]
        public void ReadEventsByChunks_WhenEmptyEventList_OneEmptyChunckReturned()
        {
            Mock<IEventSync> eventSource = new Mock<IEventSync>();
            eventSource.Setup(x => x.ReadEvents()).Returns(new AggregateRootEvent[0]);
            var chunckedList = eventSource.Object.ReadEventsByChunks();
            Assert.AreEqual(chunckedList.Count(), 0);
        }
        [Test]
        public void ReadEventsByChunks_WhenE2ItemsEventListChunckSizeIs2_OneChunckWith2EventsReturned()
        {
            Mock<IEventSync> eventSource = new Mock<IEventSync>();
            var result = new AggregateRootEvent[] {new AggregateRootEvent(), new AggregateRootEvent()};
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(2).ToList();
            Assert.AreEqual(chunckedList.Count, 1);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[0].Last(), result[1]);
        }
        [Test]
        public void ReadEventsByChunks_WhenE2ItemsEventListSameCommitIdChunckSizeIs1_OneChunckWith2EventsReturned()
        {
            Mock<IEventSync> eventSource = new Mock<IEventSync>();
            var result = new AggregateRootEvent[] { new AggregateRootEvent(), new AggregateRootEvent() };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(1).ToList();
            Assert.AreEqual(chunckedList.Count, 1);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[0].Last(), result[1]);
        }
        [Test]
        public void ReadEventsByChunks_WhenE2ItemsEventListDifferentCommitIdsChunckSizeIs1_TwoChuncksWith1EventIsReturned()
        {
            Mock<IEventSync> eventSource = new Mock<IEventSync>();
            var result = new AggregateRootEvent[] { new AggregateRootEvent() { CommitId = Guid.NewGuid() }, new AggregateRootEvent() { CommitId = Guid.NewGuid() } };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(1).ToList();
            Assert.AreEqual(chunckedList.Count, 2);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[1].First(), result[1]);
        }

        [Test]
        public void ReadEventsByChunks_3Dubl1Unique3Dubl1Uniq_3Chuncks()
        {
            Mock<IEventSync> eventSource = new Mock<IEventSync>();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var guid4 = Guid.NewGuid();
            var result = new AggregateRootEvent[]
                             {
                                 new AggregateRootEvent() { CommitId = guid1 }, 
                                 new AggregateRootEvent() { CommitId = guid1 },
                                 new AggregateRootEvent() { CommitId = guid1 },
                                 new AggregateRootEvent() { CommitId = guid2 },
                                 new AggregateRootEvent() { CommitId = guid3 },
                                 new AggregateRootEvent() { CommitId = guid3 },
                                 new AggregateRootEvent() { CommitId = guid3 },
                                 new AggregateRootEvent() { CommitId = guid4 },
                             };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(2).ToList();
            Assert.AreEqual(chunckedList.Count, 3);
            foreach (AggregateRootEvent firstChunckitem in chunckedList[0])
            {
                Assert.AreEqual(firstChunckitem.CommitId, guid1);
            }
            Assert.AreEqual(chunckedList[1].First().CommitId, guid2);
            foreach (AggregateRootEvent secondChunckitem in chunckedList[1].Skip(1))
            {
                Assert.AreEqual(secondChunckitem.CommitId, guid3);
            }
            Assert.AreEqual(chunckedList[2].First().CommitId, guid4);
        }
    }
}
