using System;
using System.Linq;
using FluentAssertions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publish_10_events_on_bus_with_functional_style_denormalizer_registered : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.Test]
        public void should_functional_denormalizer_method_handle_be_called_once_with_whole_published_stream()
        {
            var ncqrsStyleDenormalizerMock = new TestFunctionalDenormalzierNonThrowing();

            var serviceLocator = Create.Service.ServiceLocatorService(ncqrsStyleDenormalizerMock);

            var denormalizerRegistry = Create.Service.DenormalizerRegistryNative();
            denormalizerRegistry.RegisterFunctional<TestFunctionalDenormalzierNonThrowing>();

            var ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(serviceLocator: serviceLocator,
                denormalizerRegistry: denormalizerRegistry);

            var eventSourceId = Guid.NewGuid();
            var eventsToPublish = CreatePublishableEvents(10, eventSourceId).ToArray();

            // Act
            ncqrCompatibleEventDispatcher.Publish(eventsToPublish);

            // Assert

            ncqrsStyleDenormalizerMock.HandleCount.Should().Be(1);
            ncqrsStyleDenormalizerMock.TotalEvents.Should().Be(10);
        }
    }
}
