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
    internal class when_publishing_event_to_3_old_school_handlers_and_first_and_last_of_them_fail : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            publishableEvent = Create.PublishedEvent.InterviewCreated();
            secondOldSchoolEventHandlerMock = new TestDenormalzierNonThrowing();

            var handler1 = new TestDenormalzier();
            var handler2 = new TestDenormalzier1();

            var serviceLocator = Create.Service.ServiceLocatorService(secondOldSchoolEventHandlerMock, handler2, handler1);

            var denormalizerRegistry = Create.Service.DenormalizerRegistryNative();
            denormalizerRegistry.Register<TestDenormalzierNonThrowing>();
            denormalizerRegistry.Register<TestDenormalzier>();
            denormalizerRegistry.Register<TestDenormalzier1>();

            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher(serviceLocator: serviceLocator, denormalizerRegistry: denormalizerRegistry);
            
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
            secondOldSchoolEventHandlerMock.HandleCount.Should().Be(1);

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static TestDenormalzierNonThrowing secondOldSchoolEventHandlerMock;
    }
}
