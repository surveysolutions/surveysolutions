// -----------------------------------------------------------------------
// <copyright file="SnapshootDomainEventHandlerTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
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
		[ExpectedException(typeof(AggregateException))]
		public void HandleEvent_MethodRestoreFromSnapshotIsAbsentInTargetAR_ExceptionIsThrowed()
		{
			SnapshootDomainEventHandler target = new SnapshootDomainEventHandler(new object());
			target.HandleEvent(new SnapshootLoaded());
		}

		[Test]
		public void HandleEvent_AllDataIsCorrect_TrueReturned()
		{
			var aggregateRoot = new MockISnapshotable();
			SnapshootDomainEventHandler target = new SnapshootDomainEventHandler(aggregateRoot);
			var payload = new object();
			target.HandleEvent(new SnapshootLoaded() { Template = new Snapshot(Guid.NewGuid(), 1, payload) });

			Assert.That(aggregateRoot.RestoreFromSnapshotExecutionCounter, Is.EqualTo(1));
		}
	}

	public class MockISnapshotable : ISnapshotable<object>
	{
		public int RestoreFromSnapshotExecutionCounter { get; private set; }

		public object CreateSnapshot()
		{
			RestoreFromSnapshotExecutionCounter = 0;
			return new object();
		}

		public void RestoreFromSnapshot(object snapshot)
		{
			RestoreFromSnapshotExecutionCounter++;
		}
	}
}
