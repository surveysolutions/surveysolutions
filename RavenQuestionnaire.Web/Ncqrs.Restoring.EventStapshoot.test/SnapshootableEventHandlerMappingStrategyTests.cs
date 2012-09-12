// -----------------------------------------------------------------------
// <copyright file="SnapshootableEventHandlerMappingStrategyTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Moq;
using NUnit.Framework;
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
        public void GetEventHandlers_ARIsNotSnapshotable_ExceptionThrowed()
        {
            Mock<IEventHandlerMappingStrategy> baseStrategy=new Mock<IEventHandlerMappingStrategy>();
            SnapshootableEventHandlerMappingStrategy target = new SnapshootableEventHandlerMappingStrategy(baseStrategy.Object);
            Assert.Throws<AggregateException>(() => target.GetEventHandlers(new object()));
        }
        [Test]
        public void GetEventHandlers_ARIsSnapshotable_SnapshootDomainEventHandlerIsAdded()
        {
            Mock<IEventHandlerMappingStrategy> baseStrategy = new Mock<IEventHandlerMappingStrategy>();
            Mock<ISnapshotable<object>> aggregateRoot = new Mock<ISnapshotable<object>>();
            SnapshootableEventHandlerMappingStrategy target =
                new SnapshootableEventHandlerMappingStrategy(baseStrategy.Object);
            var result = target.GetEventHandlers(aggregateRoot.Object);
            Assert.True(result.Count(h => h is SnapshootDomainEventHandler) > 0);
            baseStrategy.Verify(x => x.GetEventHandlers(aggregateRoot.Object), Times.Once());
        }
    }
}
