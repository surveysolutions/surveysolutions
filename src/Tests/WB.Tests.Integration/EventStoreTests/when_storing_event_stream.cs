using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.Infrastructure.Native.Storage.EventStore.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.EventStoreTests
{
    [Subject(typeof (WriteSideEventStore))]
    public class when_storing_event_stream : with_in_memory_event_store
    {
        Establish context = () =>
        {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            int sequenceCounter = 1;
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

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

            WriteSideEventStorage = new WriteSideEventStore(
                ConnectionProvider, 
                Mock.Of<ILogger>(), 
                new EventStoreSettings { InitializeProjections = false }, 
                eventTypeResolver);
        };

        Because of = () => WriteSideEventStorage.Store(events);

        It should_read_stored_events = () =>
        {
            var eventStream = WriteSideEventStorage.Read(eventSourceId, 0);
            eventStream.Count().ShouldEqual(3);

            var firstEvent = eventStream.First();
            firstEvent.Payload.ShouldBeOfExactType<AccountRegistered>();
            var accountRegistered = (AccountRegistered)firstEvent.Payload;

            accountRegistered.Email.ShouldEqual("test@test.com");
        };

        Cleanup things = () => WriteSideEventStorage.Dispose();

        private static UncommittedEventStream events;
        private static WriteSideEventStore WriteSideEventStorage;
        private static Guid eventSourceId;
    }
}

