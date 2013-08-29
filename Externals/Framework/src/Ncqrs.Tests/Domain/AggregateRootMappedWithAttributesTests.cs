using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Mapping;
using NUnit.Framework;
using MockRepository = Rhino.Mocks.MockRepository;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class AggregateRootMappedWithAttributesTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void Initializing_one_should_set_the_mapping_strategy_to_attributed_based()
        {
            var aggregateRoot = MockRepository.GenerateMock<AggregateRootMappedWithAttributes>();
            var field = aggregateRoot.GetType().BaseType.BaseType.GetField("_mappingStrategy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            
            var theStrategy = field.GetValue(aggregateRoot);
            theStrategy.Should().BeOfType<AttributeBasedEventHandlerMappingStrategy>();
        }
    }
}
