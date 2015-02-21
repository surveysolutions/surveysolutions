using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Utils.Services;
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

            int sequenceCounter = 1;

            NcqrsEnvironment.RegisterEventDataType(typeof(AccountRegistered));
            NcqrsEnvironment.RegisterEventDataType(typeof(AccountConfirmed));
            NcqrsEnvironment.RegisterEventDataType(typeof(AccountLocked));

            events = new UncommittedEventStream(Guid.NewGuid(), null);

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

            WriteSideEventStorage = new WriteSideEventStore(ConnectionProvider, Mock.Of<ILogger>(), new EventStoreConnectionSettings{InitializeProjections = false}, new EventStoreWriteSideSettings(1024));
            WriteSideEventStorage.Store(events);
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

        Cleanup things = () => WriteSideEventStorage.Dispose();

        private static UncommittedEventStream events;
        private static WriteSideEventStore WriteSideEventStorage;
        private static Guid eventSourceId;
        private static IEnumerable<CommittedEvent> readEvents;
    }
}