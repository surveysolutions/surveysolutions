using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure.ReadSideServiceTests
{
    [TestFixture]
    internal class when_rebuild_selected_views_by_selected_event_sources : ReadSideServiceTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            readSideRepositoryWriterMock = new Mock<ICacheableRepositoryWriter>();
            readSideRepositoryWriterMock.Setup(x => x.ViewType).Returns(typeof(object));

            eventHandlerMock = new Mock<IAtomicEventHandler>();

            eventHandlerMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryWriterMock.Object });
            eventHandlerMock.Setup(x => x.Name).Returns(HandlerToRebuild);

            eventDispatcherMock = new Mock<IEventDispatcher>();
            eventDispatcherMock.Setup(x => x.GetAllRegistredEventHandlers()).Returns(new[] { eventHandlerMock.Object });

            committedEvent = new CommittedEvent(Guid.NewGuid(), "test", Guid.NewGuid(), eventSourceId, 1, DateTime.Now, 0, Mock.Of<IEvent>());
            streamableEventStoreMock = new Mock<IStreamableEventStore>();
            streamableEventStoreMock.Setup(x => x.Read(eventSourceId, 0))
                .Returns(new CommittedEventStream(eventSourceId, new[] { committedEvent }));

            transactionManagerProviderManagerMock = Mock.Get(Mock.Of<ITransactionManagerProviderManager>(_
                => _.GetTransactionManager() == Mock.Of<ITransactionManager>()));

            readSideService = CreateReadSideService(
                eventDispatcher: eventDispatcherMock.Object,
                streamableEventStore: streamableEventStoreMock.Object,
                transactionManagerProviderManager: transactionManagerProviderManagerMock.Object);

            // Act
            readSideService.RebuildViewForEventSourcesAsync(new[] { HandlerToRebuild }, new[] { eventSourceId });
            WaitRebuildReadsideFinish(readSideService);
        }

        [Test]
        public void should_rebuild_all_view() =>
            readSideService.AreViewsBeingRebuiltNow().ShouldEqual(false);

        [Test]
        public void should_call_CleanWritersByEventSource_method_for_registered_writers_and_by_passed_event_source_once() =>
            eventHandlerMock.Verify(x => x.CleanWritersByEventSource(eventSourceId), Times.Once);

        [Test]
        public void should_enable_cache_for_registered_writers_once() =>
            readSideRepositoryWriterMock.Verify(x => x.EnableCache(), Times.Once);

        [Test]
        public void should_disable_cache_for_registered_writers_once() =>
           readSideRepositoryWriterMock.Verify(x => x.DisableCache(), Times.Once);

        [Test]
        public void should_pin_readside_transaction_manager() =>
            transactionManagerProviderManagerMock.Verify(
                _ => _.PinRebuildReadSideTransactionManager(), Times.Once);

        [Test]
        public void should_unpin_transaction_manager() =>
            transactionManagerProviderManagerMock.Verify(
                _ => _.UnpinTransactionManager(), Times.Once);

        [Test]
        public void should_publish_one_event_on_event_dispatcher() =>
            eventDispatcherMock.Verify(x => x.PublishEventToHandlers(committedEvent, Moq.It.Is<Dictionary<IEventHandler, Stopwatch>>(handlers => handlers.Count() == 1 && handlers.First().Key == eventHandlerMock.Object)), Times.Once);

        [Test]
        public void should_return_readble_status() =>
            readSideService.GetRebuildStatus().CurrentRebuildStatus.ShouldContain("Rebuild views by event sources succeeded.");

        private static ReadSideService readSideService;
        private static Mock<IEventDispatcher> eventDispatcherMock;
        private static Mock<IStreamableEventStore> streamableEventStoreMock;
        private static Mock<IAtomicEventHandler> eventHandlerMock;
        private static Mock<ICacheableRepositoryWriter> readSideRepositoryWriterMock;
        private static Mock<ITransactionManagerProviderManager> transactionManagerProviderManagerMock;

        private static Guid eventSourceId = Guid.Parse("11111111111111111111111111111111");
        private static CommittedEvent committedEvent;
        private static string HandlerToRebuild = "handler to rebuild";
    }
}
