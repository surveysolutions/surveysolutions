﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    [Subject(typeof(NcqrCompatibleEventDispatcher))]
    internal class NcqrCompatibleEventDispatcherTestContext
    {
        protected static NcqrCompatibleEventDispatcher CreateNcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null)
        {
            return Create.Service.NcqrCompatibleEventDispatcher(eventBusSettings: eventBusSettings);
        }

        protected static IPublishableEvent CreatePublishableEvent(Guid? eventSourceId = null)
        {
            return Create.Fake.PublishableEvent(eventSourceId);
        }

        protected static IEnumerable<IPublishableEvent> CreatePublishableEvents(int countOfEvents, Guid? eventSourceId = null)
        {
            for (int i = 0; i < countOfEvents; i++)
            {
                yield return CreatePublishableEvent(eventSourceId ?? Guid.NewGuid());
            }
        }
    }
}
