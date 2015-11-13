using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.EventStoreTests
{
    [Subject(typeof(WriteSideEventStore))]
    internal class when_reading_ignored_event_stream : with_in_memory_event_store
    {
        Establish context = () =>
        {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            NcqrsEnvironment.RegisterEventDataType(typeof(AccountRegistered));

            events = new UncommittedEventStream(Guid.NewGuid(), null);

            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                0,
                0,
                DateTime.UtcNow,
                new AccountRegistered { ApplicationName = "App", ConfirmationToken = "token", Email = "test@test.com" }));

            WriteSideEventStorage = new WriteSideEventStore(ConnectionProvider, Mock.Of<ILogger>(),
                new EventStoreSettings
                {
                    InitializeProjections = false,
                    EventStreamsToIgnore =
                        new HashSet<string>(new[] {string.Format("WB-{0}", eventSourceId.FormatGuid())})
                });

            WriteSideEventStorage.Store(events);
        };

        Because of = () => exception = Catch.Only<InvalidOperationException>(()=> WriteSideEventStorage.ReadFrom(eventSourceId, 0, int.MaxValue));

        It should_throw_InvalidOperationException = () => exception.ShouldNotBeNull();

        Cleanup things = () => WriteSideEventStorage.Dispose();

        private static UncommittedEventStream events;
        private static WriteSideEventStore WriteSideEventStorage;
        private static Guid eventSourceId;
        private static InvalidOperationException exception;
    }
}