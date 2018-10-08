using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_10_events_on_bus_with_functional_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        private class FunctionalEventHandlerEvent : IEvent  { }

        private class FunctionalEventHandler :
            AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideStorage<IReadSideRepositoryEntity>>,
            IUpdateHandler<IReadSideRepositoryEntity, FunctionalEventHandlerEvent>
        {
            public FunctionalEventHandler(IReadSideStorage<IReadSideRepositoryEntity> readSideStorage) : base(readSideStorage)
            {
            }

            public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity state, IPublishedEvent<FunctionalEventHandlerEvent> @event)
            {
                return state;
            }

            public new void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
            {
                this.PublishableEvents = publishableEvents;
                this.EventSourceId = eventSourceId;

                base.Handle(publishableEvents, eventSourceId);
            }

            public Guid EventSourceId { get; set; }

            public IEnumerable<IPublishableEvent> PublishableEvents { get; set; }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {

            secondFunctionalEventHandler = new FunctionalEventHandler(Mock.Of<IReadSideStorage<IReadSideRepositoryEntity>>());

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance(secondFunctionalEventHandler.GetType())).Returns(secondFunctionalEventHandler);

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(serviceLocator:serviceLocator.Object);
            ncqrCompatibleEventDispatcher.Register(secondFunctionalEventHandler);

            eventSourceId = Guid.NewGuid();

            eventsToPublish = new List<IPublishableEvent>();
            for (int i = 0; i < 10; i++)
            {
                eventsToPublish.Add(CreatePublishableEvent(eventSourceId, new FunctionalEventHandlerEvent(){}));
            }
            
            BecauseOf();
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Publish(eventsToPublish);

        [NUnit.Framework.Test]
        public void should_functional_denormalizer_method_handle_be_called_once_with_EventSourceId() =>
            Assert.That(secondFunctionalEventHandler.EventSourceId == eventSourceId);

/*

            functionalStyleEventHandlerMock.Verify(
                handler => handler.Handle(Moq.It.Is<IEnumerable<IPublishableEvent>>(events => events.SequenceEqual(eventsToPublish)), eventSourceId),
                Times.Once());*/

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static FunctionalEventHandler secondFunctionalEventHandler;
        private static List<IPublishableEvent> eventsToPublish;
        private static Guid eventSourceId;
    }
}
