﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideServiceTests
{
    internal class when_rebuild_selected_views_by_selected_event_sources : RavenReadSideServiceTestContext
    {
        Establish context = () =>
        {
            readSideRepositoryWriterMock = new Mock<IReadSideRepositoryWriter>();
            readSideRepositoryWriterMock.Setup(x => x.ViewType).Returns(typeof(object));

            eventHandlerMock = new Mock<IAtomicEventHandler>();

            eventHandlerMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryWriterMock.Object });
            eventHandlerMock.Setup(x => x.Name).Returns(HandlerToRebuild);

            eventDispatcherMock = new Mock<IEventDispatcher>();
            eventDispatcherMock.Setup(x => x.GetAllRegistredEventHandlers()).Returns(new[] { eventHandlerMock.Object });

            committedEvent = new CommittedEvent(Guid.NewGuid(), "test", Guid.NewGuid(), eventSourceId , 1, DateTime.Now, new object(),
                new Version(1, 2));
            streamableEventStoreMock = new Mock<IStreamableEventStore>();
            streamableEventStoreMock.Setup(x => x.ReadFrom(eventSourceId, 0, long.MaxValue))
                .Returns(new CommittedEventStream(eventSourceId, new[] { committedEvent }));

            ravenReadSideService = CreateRavenReadSideService(eventDispatcher: eventDispatcherMock.Object, streamableEventStore: streamableEventStoreMock.Object);
        };

        Because of = () => WaitRebuildReadsideFinish();

        It should_rebuild_all_view = () =>
            ravenReadSideService.AreViewsBeingRebuiltNow().ShouldEqual(false);

        It should_call_CleanWritersByEventSource_method_for_registered_writers_and_by_passed_event_source_once = () =>
            eventHandlerMock.Verify(x => x.CleanWritersByEventSource(eventSourceId), Times.Once);

        It should_enable_cache_for_registered_writers_once = () =>
            readSideRepositoryWriterMock.Verify(x => x.EnableCache(), Times.Once);

        It should_disable_cache_for_registered_writers_once = () =>
           readSideRepositoryWriterMock.Verify(x => x.DisableCache(), Times.Once);

        It should_publish_one_event_on_event_dispatcher = () =>
            eventDispatcherMock.Verify(x => x.PublishEventToHandlers(committedEvent, Moq.It.Is<IEnumerable<IEventHandler>>(handlers => handlers.Count() == 1 && handlers.First() == eventHandlerMock.Object)), Times.Once);

        It should_return_readble_status = () =>
            ravenReadSideService.GetRebuildStatus().CurrentRebuildStatus.ShouldContain("Rebuild specific views succeeded.");

        private static RavenReadSideService ravenReadSideService;
        private static Mock<IEventDispatcher> eventDispatcherMock;
        private static Mock<IStreamableEventStore> streamableEventStoreMock;
        private static Mock<IAtomicEventHandler> eventHandlerMock;
        private static Mock<IReadSideRepositoryWriter> readSideRepositoryWriterMock;

        private static Guid eventSourceId = Guid.Parse("11111111111111111111111111111111");
        private static CommittedEvent committedEvent;
        private static string HandlerToRebuild = "handler to rebuild";

        protected static void WaitRebuildReadsideFinish()
        {
            ravenReadSideService.RebuildViewForEventSourcesAsync(new[] { HandlerToRebuild }, new[] { eventSourceId });

            Thread.Sleep(1000);

            while (ravenReadSideService.AreViewsBeingRebuiltNow())
            {
                Thread.Sleep(1000);
            }
        }
    }
}
