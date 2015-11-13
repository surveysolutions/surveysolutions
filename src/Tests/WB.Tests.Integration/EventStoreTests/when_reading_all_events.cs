using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.EventStoreTests
{
    [Subject(typeof (WriteSideEventStore))]
    public class when_reading_all_events : with_in_memory_event_store
    {
        Establish context = () =>
        {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            ignoredEventSourceId=Guid.NewGuid();

            int sequenceCounter = 1;
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

            events = new UncommittedEventStream(Guid.NewGuid(), null);
            var ignoredEventStream= new UncommittedEventStream(Guid.NewGuid(), null);
            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId, 
                sequenceCounter++, 
                0, 
                DateTime.UtcNow, 
                new AccountRegistered{ApplicationName = "App", ConfirmationToken = "token", Email = "test@test.com"}));

            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId, 
                sequenceCounter++, 
                0, 
                DateTime.UtcNow, 
                new AccountConfirmed()));
            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId,
                sequenceCounter++,
                0, 
                DateTime.UtcNow,
                new AccountLocked()));
                
   			ignoredEventStream.Append(new UncommittedEvent(Guid.NewGuid(),
               ignoredEventSourceId,
               0,
               0,
               DateTime.UtcNow,
               new AccountRegistered { ApplicationName = "App", ConfirmationToken = "token", Email = "test@test.com" }));

            WriteSideEventStorage = new WriteSideEventStore(ConnectionProvider, Mock.Of<ILogger>(),
            new EventStoreSettings {
            InitializeProjections = false,
            EventStreamsToIgnore =
            new HashSet<string>(new[] {string.Format("WB-{0}", ignoredEventSourceId.FormatGuid())})}, eventTypeResolver);
            WriteSideEventStorage.Store(events);
            WriteSideEventStorage.Store(ignoredEventStream);
        };

        Because of = () => readEvents = WriteSideEventStorage.GetAllEvents();

        It should_read_stored_events = () =>
        {
            readEvents.Any().ShouldBeTrue();
            readEvents.Count().ShouldEqual(3);

            var firstEvent = readEvents.First();
            firstEvent.Payload.ShouldBeOfExactType<AccountRegistered>();
            var accountRegistered = (AccountRegistered)firstEvent.Payload;

            accountRegistered.Email.ShouldEqual("test@test.com");
        };

        It should_not_read_events_by_ignored_event_stream =
            () => readEvents.Where(e => e.EventSourceId == ignoredEventSourceId).ShouldBeEmpty();

        Cleanup things = () => WriteSideEventStorage.Dispose();

        private static UncommittedEventStream events;
        private static WriteSideEventStore WriteSideEventStorage;
        private static Guid eventSourceId;
        private static Guid ignoredEventSourceId;
        private static IEnumerable<CommittedEvent> readEvents;
    }
}