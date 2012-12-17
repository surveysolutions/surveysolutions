// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventSyncExtensionsTests.cs" company="">
//   
// </copyright>
// <summary>
//   The event sync extensions tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Tests.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The event sync extensions tests.
    /// </summary>
    [TestFixture]
    public class EventSyncExtensionsTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read events by chunks_3 dubl 1 unique 3 dubl 1 uniq_3 chuncks.
        /// </summary>
        [Test]
        public void ReadEventsByChunks_3Dubl1Unique3Dubl1Uniq_3Chuncks()
        {
            var eventSource = new Mock<IEventStreamReader>();
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();
            Guid guid4 = Guid.NewGuid();
            var result = new[]
                {
                    new AggregateRootEvent { CommitId = guid1 }, new AggregateRootEvent { CommitId = guid1 }, 
                    new AggregateRootEvent { CommitId = guid1 }, new AggregateRootEvent { CommitId = guid2 }, 
                    new AggregateRootEvent { CommitId = guid3 }, new AggregateRootEvent { CommitId = guid3 }, 
                    new AggregateRootEvent { CommitId = guid3 }, new AggregateRootEvent { CommitId = guid4 }, 
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

        /// <summary>
        /// The read events by chunks_ when e 2 items event list chunck size is 2_ one chunck with 2 events returned.
        /// </summary>
        [Test]
        public void ReadEventsByChunks_WhenE2ItemsEventListChunckSizeIs2_OneChunckWith2EventsReturned()
        {
            var eventSource = new Mock<IEventStreamReader>();
            var result = new[] { new AggregateRootEvent(), new AggregateRootEvent() };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(2).ToList();
            Assert.AreEqual(chunckedList.Count, 1);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[0].Last(), result[1]);
        }

        /// <summary>
        /// The read events by chunks_ when e 2 items event list different commit ids chunck size is 1_ two chuncks with 1 event is returned.
        /// </summary>
        [Test]
        public void
            ReadEventsByChunks_WhenE2ItemsEventListDifferentCommitIdsChunckSizeIs1_TwoChuncksWith1EventIsReturned()
        {
            var eventSource = new Mock<IEventStreamReader>();
            var result = new[]
                {
                    new AggregateRootEvent { CommitId = Guid.NewGuid() }, 
                    new AggregateRootEvent { CommitId = Guid.NewGuid() }
                };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(1).ToList();
            Assert.AreEqual(chunckedList.Count, 2);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[1].First(), result[1]);
        }

        /// <summary>
        /// The read events by chunks_ when e 2 items event list same commit id chunck size is 1_ one chunck with 2 events returned.
        /// </summary>
        [Test]
        public void ReadEventsByChunks_WhenE2ItemsEventListSameCommitIdChunckSizeIs1_OneChunckWith2EventsReturned()
        {
            var eventSource = new Mock<IEventStreamReader>();
            var result = new[] { new AggregateRootEvent(), new AggregateRootEvent() };
            eventSource.Setup(x => x.ReadEvents()).Returns(result);
            var chunckedList = eventSource.Object.ReadEventsByChunks(1).ToList();
            Assert.AreEqual(chunckedList.Count, 1);
            Assert.AreEqual(chunckedList[0].First(), result[0]);
            Assert.AreEqual(chunckedList[0].Last(), result[1]);
        }

        /// <summary>
        /// The read events by chunks_ when empty event list_ one empty chunck returned.
        /// </summary>
        [Test]
        public void ReadEventsByChunks_WhenEmptyEventList_OneEmptyChunckReturned()
        {
            var eventSource = new Mock<IEventStreamReader>();
            eventSource.Setup(x => x.ReadEvents()).Returns(new AggregateRootEvent[0]);
            var chunckedList = eventSource.Object.ReadEventsByChunks();
            Assert.AreEqual(chunckedList.Count(), 0);
        }

        #endregion
    }
}