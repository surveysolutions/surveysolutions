// -----------------------------------------------------------------------
// <copyright file="SnapshootableAggregateRootTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using NUnit.Framework;
using Ncqrs.Eventing;
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
	public class SnapshootableAggregateRootTests
	{
		[Test]
		public void InitializeFromHistory_SnapsotsAreAbsent_RestoredFromScratch()
		{
			Guid aggreagateRootId = Guid.NewGuid();
			CommittedEventStream eventStream = new CommittedEventStream(aggreagateRootId,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 1,
																						   DateTime.Now,
																						   new object(),
																						   new Version()));
			DummyAR aggregateRoot = new DummyAR();
			aggregateRoot.InitializeFromHistory(eventStream);
			Assert.True(aggregateRoot.EventHandlingCounter == 1);
		}
		[Test]
		public void InitializeFromHistory_OneSnatshotIsAvalible_RestoredFromSnapshotToTheEnd()
		{
			Guid aggreagateRootId = Guid.NewGuid();
			CommittedEventStream eventStream = new CommittedEventStream(aggreagateRootId,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 1,
																						   DateTime.Now,
																						   new object(),
																						   new Version())
																		,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 2,
																						   DateTime.Now,
																						   new SnapshootLoaded()
																						   {
																							   Template =
																								   new Snapshot(
																								   aggreagateRootId,
																								   2, new object())
																						   },
																						   new Version())
																		,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 3,
																						   DateTime.Now,
																						   new object(),
																						   new Version()));
			DummyAR aggregateRoot = new DummyAR();
			aggregateRoot.InitializeFromHistory(eventStream);
			Assert.True(aggregateRoot.EventHandlingCounter == 1);
			Assert.True(aggregateRoot.RestoreFromSnapshotCounter == 1);
		}
		[Test]
		public void InitializeFromHistory_TwoSnatshotsAreAvalible_RestoredFromLastSnapshotToTheEnd()
		{
			Guid aggreagateRootId = Guid.NewGuid();
			CommittedEventStream eventStream = new CommittedEventStream(aggreagateRootId,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 1,
																						   DateTime.Now,
																						   new SnapshootLoaded()
																						   {
																							   Template =
																								   new Snapshot(
																								   aggreagateRootId,
																								   1, new object())
																						   },
																						   new Version())
																		,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 2,
																						   DateTime.Now,
																						   new SnapshootLoaded()
																						   {
																							   Template =
																								   new Snapshot(
																								   aggreagateRootId,
																								   2, new object())
																						   },
																						   new Version())
																		,
																		new CommittedEvent(Guid.NewGuid(),
																						   Guid.NewGuid(),
																						   aggreagateRootId, 3,
																						   DateTime.Now,
																						   new object(),
																						   new Version()));
			DummyAR aggregateRoot = new DummyAR();
			aggregateRoot.InitializeFromHistory(eventStream);
			Assert.True(aggregateRoot.EventHandlingCounter == 1);
			Assert.True(aggregateRoot.RestoreFromSnapshotCounter == 1);
		}
		class DummyAR : SnapshootableAggregateRoot<object>
		{

			public int EventHandlingCounter { get; private set; }
			public int RestoreFromSnapshotCounter { get; private set; }
			protected void OnDummyEventHandler(object evt)
			{
				this.EventHandlingCounter++;
			}

			#region Overrides of SnapshootableAggregateRoot<object>

			public override object CreateSnapshot()
			{
				throw new NotImplementedException();
			}

			public override void RestoreFromSnapshot(object snapshot)
			{
				this.RestoreFromSnapshotCounter++;
			}

			#endregion
		}
	}

}
