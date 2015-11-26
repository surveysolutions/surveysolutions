﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_by_ignored_event_source : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            var secondEventHandlerMock = new Mock<IEventHandler>();
            secondOldSchoolEventHandlerMock = secondEventHandlerMock.As<IEventHandler<ILiteEvent>>();
            ncqrCompatibleEventDispatcher =
                CreateNcqrCompatibleEventDispatcher(new EventBusSettings()
                {
                    IgnoredAggregateRoots = new HashSet<string>(new[] {ignoredEvenetSource.FormatGuid()})
                });

            ncqrCompatibleEventDispatcher.Register(secondEventHandlerMock.Object);
        };

        Because of = () => ncqrCompatibleEventDispatcher.Publish(new[] { Create.PublishableEvent(eventSourceId:ignoredEvenetSource) });

        It should_not_call_registred_event_handler = () =>
         secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
             Moq.It.IsAny<IPublishedEvent<ILiteEvent>>()),
             Times.Never);

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Guid ignoredEvenetSource=Guid.NewGuid();
        private static Mock<IEventHandler<ILiteEvent>> secondOldSchoolEventHandlerMock;
    }
}