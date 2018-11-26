using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_2_functional_handlers_and_second_catch_non_critical_exception : NcqrCompatibleEventDispatcherTestContext
    {
        private class FunctionalEventHandlerEvent : IEvent { }

        private class FunctionalEventHandler :
            AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideStorage<IReadSideRepositoryEntity>>,
            IUpdateHandler<IReadSideRepositoryEntity, FunctionalEventHandlerEvent>
        {
            public FunctionalEventHandler(IReadSideStorage<IReadSideRepositoryEntity> readSideStorage) : base(readSideStorage)
            {
            }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity state, IPublishedEvent<FunctionalEventHandlerEvent> @event)
            {
                throw new NotImplementedException();
            }
        }

        private class FailingFunctionalEventHandler :
            AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideStorage<IReadSideRepositoryEntity>>,
            IUpdateHandler<IReadSideRepositoryEntity, FunctionalEventHandlerEvent>
        {
            public FailingFunctionalEventHandler(IReadSideStorage<IReadSideRepositoryEntity> readSideStorage) : base(readSideStorage)
            {
            }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity state, IPublishedEvent<FunctionalEventHandlerEvent> @event)
            {
                throw new NotImplementedException();
            }

            public new void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
            {
                throw new Exception();
            }

        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            publishableEvent = Create.Fake.PublishableEvent(payload: new FunctionalEventHandlerEvent());

            var failingFunctionalEventHandler = new FailingFunctionalEventHandler(Mock.Of<IReadSideStorage<IReadSideRepositoryEntity>>());
            var secondFunctionalEventHandler = new FunctionalEventHandler(Mock.Of<IReadSideStorage<IReadSideRepositoryEntity>>());

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance(secondFunctionalEventHandler.GetType())).Returns(secondFunctionalEventHandler);
            serviceLocator.Setup(x => x.GetInstance(failingFunctionalEventHandler.GetType())).Returns(failingFunctionalEventHandler);

            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher(logger: loggerMock.Object,
                eventBusSettings: new EventBusSettings()
                {
                    EventHandlerTypesWithIgnoredExceptions = new[]
                    {
                        secondFunctionalEventHandler.GetType()
                    },
                    DisabledEventHandlerTypes = new Type[0]
                },
                serviceLocator: serviceLocator.Object);
            eventDispatcher.OnCatchingNonCriticalEventHandlerException +=
                (e) => { handledNonCriticalEventHandlerException = e; };

            eventDispatcher.Register(failingFunctionalEventHandler);
            eventDispatcher.Register(secondFunctionalEventHandler);
            BecauseOf();
        }

        public void BecauseOf() =>
            aggregateException = Assert.Throws<AggregateException>(() =>
                eventDispatcher.Publish(publishableEvent.ToEnumerable().ToArray()));

        [NUnit.Framework.Test] public void should_throw_AggregateException () =>
            aggregateException.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_put_1_exception_to_AggregateException () =>
            aggregateException.InnerExceptions.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_log_catched_exception () =>
            loggerMock.Verify(x => x.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Times.Once);

        [NUnit.Framework.Test] public void should_be_handled_event_handler_exception () =>
            handledNonCriticalEventHandlerException.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_not_event_handler_exception_be_critical () =>
            handledNonCriticalEventHandlerException.IsCritical.Should().BeFalse();

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static EventHandlerException handledNonCriticalEventHandlerException;
        private static readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
    }
}
