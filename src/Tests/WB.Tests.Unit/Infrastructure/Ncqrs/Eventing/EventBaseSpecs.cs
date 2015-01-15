using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using Rhino.Mocks;
using WB.Tests.Unit;
using MockRepository = Rhino.Mocks.MockRepository;

namespace Ncqrs.Tests.Eventing
{
    [TestFixture]
    public class EventBaseSpecs
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void Constructing_a_new_event_base_it_should_call_the_GenerateNewId_method_from_the_generator_that_has_been_set_in_the_environment()
        {
            var generator = MockRepository.GenerateMock<IUniqueIdentifierGenerator>();
            NcqrsEnvironment.SetDefault<IUniqueIdentifierGenerator>(generator);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(() => generator);

            var mock = MockRepository.GenerateStub<Event>();

            generator.AssertWasCalled(g=>g.GenerateNewId());
        }

        [Test]
        public void Constructing_a_new_event_base_it_should_set_the_event_identifier_to_identifier_that_has_been_given_from_the_IUniqueIdentifierGenerator_from_the_NcqrsEnvironment()
        {
            var identiefier = Guid.NewGuid();

            var generator = MockRepository.GenerateStrictMock<IUniqueIdentifierGenerator>();
            generator.Stub(g => g.GenerateNewId()).Return(identiefier);

            NcqrsEnvironment.SetDefault<IUniqueIdentifierGenerator>(generator);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(() => generator);

            var mock = MockRepository.GenerateStub<Event>();
            mock.EventIdentifier.Should().Be(identiefier);
        }
    }
}
