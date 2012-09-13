using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class SyncprocessTests
    {
        [Test]
        public void AddEvent_ProcessIsFinished_InvalidOperationExceptionThrowed()
        {
            SyncProcess process = new SyncProcess(new SyncProcessDocument() {EndDate = DateTime.UtcNow});
            Assert.Throws<InvalidOperationException>(() => process.AddEvents(new EventDocument[0]));
        }
        [Test]
        public void AddEvent_ProcessIsStarted_EventsAreAdeedInInitialState()
        {
            SyncProcess process = new SyncProcess(Guid.NewGuid());
            var events = new List<EventDocument> {new EventDocument()};
            process.AddEvents(events);
            var innerDoc = process.GetInnerDocument();
            Assert.AreEqual(innerDoc.Events.Count, events.Count);
            for(int i=0;i<events.Count;i++)
            {
                Assert.AreEqual(events[i], innerDoc.Events[i].Event);
                Assert.AreEqual(innerDoc.Events[i].Handled, EventState.Initial);
            }
        }
        [Test]
        public void SetEventState_ProcessIsFinished_InvalidOperationExceptionThrowed()
        {
            SyncProcess process = new SyncProcess(new SyncProcessDocument() { EndDate = DateTime.UtcNow });
            Assert.Throws<InvalidOperationException>(() => process.SetEventState(Guid.NewGuid(), EventState.Initial));
        }
        [Test]
        public void SetEventState_EventIsAbsent_ArgumentExeptionThrowed()
        {
            SyncProcess process = new SyncProcess(Guid.NewGuid());
            Assert.Throws<ArgumentException>(() => process.SetEventState(Guid.NewGuid(), EventState.Initial));
        }
        [Test]
        public void SetEventState_EventIspresent_IStateIsChanged()
        {
            var innerDoc = new SyncProcessDocument();
            var eventGuid = Guid.NewGuid();
            var targetEvent = new ProcessedEvent(new EventDocument(new DeleteUserCommand("id", null), eventGuid, Guid.NewGuid()));
            
            innerDoc.Events.Add(targetEvent);
            SyncProcess process = new SyncProcess(innerDoc);
            process.SetEventState(eventGuid,EventState.Error);
            Assert.AreEqual(targetEvent.Handled, EventState.Error);
        }
        [Test]
        public void EndProcess_ProcessIsFinished_InvalidOperationExceptionThrowed()
        {
            SyncProcess process = new SyncProcess(new SyncProcessDocument() { EndDate = DateTime.UtcNow });
            Assert.Throws<InvalidOperationException>(process.EndProcess);
        }
        [Test]
        public void EndProcess_ProcessInProgress_ProcessIsCompleted()
        {
            SyncProcess process = new SyncProcess(Guid.NewGuid());
            var innerDoc = process.GetInnerDocument();
            Assert.False(innerDoc.EndDate.HasValue);
            process.EndProcess();
            Assert.True(innerDoc.EndDate.HasValue);
        }
    }
}
