using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.Synchronization.Events.Sync;
using WB.Tests.Abc;
using WB.Tests.Unit;

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

        [Test]
        public void should_find_apply_method_for_interview_created_event()
        {
            ConventionBasedEventHandlerMappingStrategy strategy = new ConventionBasedEventHandlerMappingStrategy();

            var canHandle = strategy.CanHandleEvent(Create.AggregateRoot.StatefulInterview(interviewId:Id.g2, shouldBeInitialized: false), typeof(InterviewCreated));
            Assert.That(canHandle, Is.True);
        }
    }
}