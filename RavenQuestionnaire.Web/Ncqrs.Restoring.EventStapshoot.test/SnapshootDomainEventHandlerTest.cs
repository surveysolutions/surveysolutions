// -----------------------------------------------------------------------
// <copyright file="SnapshootDomainEventHandlerTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Restoring.EventStapshoot.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class SnapshootDomainEventHandlerTest
    {
        [Test]
        public void HandleEvent_EventISNotSnapshootLoaded_FalseReturned()
        {
            SnapshootDomainEventHandler target = new SnapshootDomainEventHandler(new object());
            Assert.False(target.HandleEvent(new object()));
        }
        [Test]
        public void HandleEvent_MethodRestoreFromSnapshotIsAbsentInTargetAR_ExceptionIsThrowed()
        {
            SnapshootDomainEventHandler target = new SnapshootDomainEventHandler(new object());
            Assert.Throws<AggregateException>(() => target.HandleEvent(new SnapshootLoaded()));
        }
        [Test]
        public void HandleEvent_AllDataIsCorrect_TrueReturned()
        {
            Mock<ISnapshotable<object>> aggregateRoot = new Mock<ISnapshotable<object>>();
            SnapshootDomainEventHandler target = new SnapshootDomainEventHandler(aggregateRoot.Object);
            var payload = new object();
            target.HandleEvent(new SnapshootLoaded() {Template = new Snapshot(Guid.NewGuid(), 1, payload)});
            aggregateRoot.Verify(x => x.RestoreFromSnapshot(payload), Times.Once());
        }
    }
}
