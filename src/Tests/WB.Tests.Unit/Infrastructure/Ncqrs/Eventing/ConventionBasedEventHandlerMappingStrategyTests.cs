using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.Synchronization.Events.Sync;

namespace Ncqrs.Tests.Eventing
{
    [TestOf(typeof(ConventionBasedEventHandlerMappingStrategy))]
    public class ConventionBasedEventHandlerMappingStrategyTests
    {
        [Test]
        public void should_find_apply_methods_when_they_are_protected()
        {
            ConventionBasedEventHandlerMappingStrategy strategy = new ConventionBasedEventHandlerMappingStrategy();

            var canHandle = strategy.CanHandleEvent(new Tablet(), typeof(TabletRegistered));
            Assert.That(canHandle, Is.True);
        }
    }
}