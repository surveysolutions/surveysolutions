using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Mapping;
using NUnit.Framework;
using AndroidMocks;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class AggregateRootMappedWithAttributesTests
    {
        [Test]
        public void Initializing_one_should_set_the_mapping_strategy_to_attributed_based()
        {
            var aggregateRoot = new DynamicMock<AggregateRootMappedWithAttributes>();
            var field = aggregateRoot.Instance.GetType().BaseType.BaseType.GetField("_mappingStrategy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            
            var theStrategy = field.GetValue(aggregateRoot);
            theStrategy.Should().BeOfType<AttributeBasedEventHandlerMappingStrategy>();
        }
    }
}
