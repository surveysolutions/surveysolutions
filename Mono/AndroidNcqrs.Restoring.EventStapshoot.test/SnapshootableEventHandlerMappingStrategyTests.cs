// -----------------------------------------------------------------------
// <copyright file="SnapshootableEventHandlerMappingStrategyTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
using NUnit.Framework;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;
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
	public class SnapshootableEventHandlerMappingStrategyTests
	{
		[Test]
		[ExpectedException(typeof(AggregateException))]
		public void GetEventHandlers_ARIsNotSnapshotable_ExceptionThrowed()
		{
			var baseStrategy = new MockIEventHandlerMappingStrategy();
			SnapshootableEventHandlerMappingStrategy target = new SnapshootableEventHandlerMappingStrategy(baseStrategy);
			target.GetEventHandlers(new object());
		}
		[Test]
		public void GetEventHandlers_ARIsSnapshotable_SnapshootDomainEventHandlerIsAdded()
		{
			var baseStrategy = new MockIEventHandlerMappingStrategy();
			var aggregateRoot = new MockISnapshotable();
			SnapshootableEventHandlerMappingStrategy target =
				new SnapshootableEventHandlerMappingStrategy(baseStrategy);
			var result = target.GetEventHandlers(aggregateRoot);
			Assert.True(result.Count(h => h is SnapshootDomainEventHandler) > 0);

			Assert.That(baseStrategy.GetEventHandlersExecutionHandler, Is.EqualTo(1));
		}
	}

	public class MockIEventHandlerMappingStrategy : IEventHandlerMappingStrategy
	{
		public int GetEventHandlersExecutionHandler { get; set; }

		public MockIEventHandlerMappingStrategy()
		{
			GetEventHandlersExecutionHandler = 0;
		}

		public IEnumerable<ISourcedEventHandler> GetEventHandlers(object target)
		{
			GetEventHandlersExecutionHandler++;
			return new List<ISourcedEventHandler>();
		}
	}
}
