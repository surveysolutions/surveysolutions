using System;
using FluentAssertions;
using Ncqrs.Eventing;
using NUnit.Framework;
using AndroidMocks;

namespace Ncqrs.Tests.Eventing
{
    [TestFixture]
    public class EventBaseSpecs
    {
		class FakeEvent : Event { }

        [Test]
        public void Constructing_a_new_event_base_it_should_call_the_GenerateNewId_method_from_the_generator_that_has_been_set_in_the_environment()
        {
            var generator = new DynamicMock<IUniqueIdentifierGenerator>();
            generator.Expect(g => g.GenerateNewId(), Guid.NewGuid());

			NcqrsEnvironment.SetDefault<IUniqueIdentifierGenerator>(generator.Instance);

	        var mock = new FakeEvent();

            generator.AssertWasCalled(g=>g.GenerateNewId());
        }

        [Test]
        public void Constructing_a_new_event_base_it_should_set_the_event_identifier_to_identifier_that_has_been_given_from_the_IUniqueIdentifierGenerator_from_the_NcqrsEnvironment()
        {
            var identiefier = Guid.NewGuid();

            var generator = new DynamicMock<IUniqueIdentifierGenerator>();
	        generator.Stub(g => g.GenerateNewId(), identiefier);

            NcqrsEnvironment.SetDefault<IUniqueIdentifierGenerator>(generator.Instance);

	        var mock = new FakeEvent();
            mock.EventIdentifier.Should().Be(identiefier);
        }

        [Test]
        public void Constructing_a_new_event_base_it_should_set_the_event_time_stap_to_the_time_given_by_the_IClock_from_the_NcqrsEnvironment()
        {
            var theTimeStamp = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            var clock = new DynamicMock<IClock>();
	        clock.Stub(c => c.UtcNow(), theTimeStamp);

            NcqrsEnvironment.SetDefault<IClock>(clock.Instance);

	        var eventBase = new FakeEvent();
            eventBase.EventTimeStamp.Should().Be(theTimeStamp);
        }
    }
}
