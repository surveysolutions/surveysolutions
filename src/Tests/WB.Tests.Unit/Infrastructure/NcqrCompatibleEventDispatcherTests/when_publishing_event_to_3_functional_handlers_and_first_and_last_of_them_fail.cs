using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_3_functional_handlers_and_first_and_last_of_them_fail : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            publishableEvent = Create.PublishedEvent.InterviewCreated();

            secondFunctionalEventHandlerMock = new TestFunctionalDenormalzierNonThrowing();

            var handler1 = new TestDenormalzier();
            var handler2 = new TestDenormalzier1();

            var serviceLocator = Create.Service.ServiceLocatorService(secondFunctionalEventHandlerMock, handler2, handler1);

            var eventRegistry = Create.Service.DenormalizerRegistryNative();
            eventRegistry.RegisterFunctional<TestFunctionalDenormalzierNonThrowing>();
            eventRegistry.Register<TestDenormalzier>();
            eventRegistry.Register<TestDenormalzier1>();


            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher(serviceLocator: serviceLocator,
                denormalizerRegistry: eventRegistry);

            BecauseOf();
        }

        public void BecauseOf() =>
            aggregateException = Assert.Throws<AggregateException>(() =>
                eventDispatcher.Publish(publishableEvent.ToEnumerable().ToArray()));

        [NUnit.Framework.Test]
        public void should_throw_AggregateException() =>
            aggregateException.Should().NotBeNull();

        [NUnit.Framework.Test]
        public void should_put_2_exceptions_to_AggregateException() =>
            aggregateException.InnerExceptions.Count.Should().Be(2);

        [NUnit.Framework.Test]
        public void should_call_second_event_handler() =>
            secondFunctionalEventHandlerMock.HandleCount.Should().Be(1);

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static TestFunctionalDenormalzierNonThrowing secondFunctionalEventHandlerMock;
    }
}
