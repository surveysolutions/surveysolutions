using System;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerEventStorage : IEventStore
    {
        private readonly IAsyncPlainStorage<EventView> eventRepository;
        private readonly ISerializer serializer;

        public InterviewerEventStorage(IAsyncPlainStorage<EventView> eventRepository, ISerializer serializer)
        {
            this.eventRepository = eventRepository;
            this.serializer = serializer;
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            var events = this.eventRepository.Query(
                eventViews => eventViews.Where(
                    eventView => eventView.EventSourceId == id && eventView.EventSequence >= minVersion && eventView.EventSequence <= maxVersion)
                    .ToList());
            
            return new CommittedEventStream(id, events.Select(ToCommitedEvent));
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            var storedEvents = eventStream.Select(ToStoredEvent).ToList();
            this.eventRepository.StoreAsync(storedEvents).Wait();

            return new CommittedEventStream(eventStream.SourceId, storedEvents.Select(ToCommitedEvent));
        }

        private CommittedEvent ToCommitedEvent(EventView storedEvent)
        {
            return new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: storedEvent.OID,
                payload: this.ToEvent(storedEvent.JsonEvent));
        }

        private EventView ToStoredEvent(UncommittedEvent evt)
        {
            return new EventView
            {
                Id = evt.EventIdentifier.FormatGuid(),
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = this.serializer.Serialize(evt.Payload, TypeSerializationSettings.AllTypes)
            };
        }

        private Infrastructure.EventBus.IEvent ToEvent(string json)
        {
            var replaceOldAssemblyNames = json.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");
            replaceOldAssemblyNames =
                new[]
                {
                    "NewUserCreated", "UserChanged", "UserLocked", "UserLockedBySupervisor", "UserUnlocked",
                    "UserUnlockedBySupervisor"
                }.Aggregate(replaceOldAssemblyNames,
                    (current, type) =>
                        current.Replace($"Main.Core.Events.User.{type}, Main.Core",
                            $"Main.Core.Events.User.{type}, WB.Core.SharedKernels.DataCollection"));

            return this.serializer.Deserialize<Infrastructure.EventBus.IEvent>(replaceOldAssemblyNames);
        }
    }
}