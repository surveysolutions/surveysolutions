using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Services.Export.Events.Interview;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.Services.TenantApi
{
    public class EventHandlerTests
    {
        [Test]
        public async Task can_resolve_all_handlers()
        {
            var calledEvents = new HashSet<(IEvent, Guid)>();
            IFunctionalHandler testSubj = new TestHandler(calledEvents);

            var events = new[]
            {
                new Event {Payload = new AnswersRemoved(), EventSourceId = Id.g1 },
                new Event {Payload = new AnswersRemoved(), EventSourceId = Id.g2},
                new Event {Payload = new InterviewHardDeleted(), EventSourceId = Id.g2}
            };

            foreach (var ev in events)
            {
                await testSubj.Handle(ev);
            }

            Assert.That(calledEvents.Contains((events[0].Payload, events[0].EventSourceId)));
            Assert.That(calledEvents.Contains((events[1].Payload, events[1].EventSourceId)));
            Assert.That(calledEvents.Contains((events[2].Payload, events[2].EventSourceId)));
        }

        class TestHandler : IFunctionalHandler,
            IEventHandler<InterviewCreated>,
            IEventHandler<AnswersRemoved>,
            IAsyncEventHandler<InterviewHardDeleted>
        {
            private readonly HashSet<(IEvent, Guid)> track;

            public TestHandler(HashSet<(IEvent, Guid)> track)
            {
                this.track = track;
            }

            public Task SaveStateAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Handle(PublishedEvent<InterviewCreated> @event)
            {
                track.Add((@event.Event, @event.EventSourceId));
            }

            public void Handle(PublishedEvent<AnswersRemoved> @event)
            {
                track.Add((@event.Event, @event.EventSourceId));
            }

            public Task Handle(PublishedEvent<InterviewHardDeleted> @event, CancellationToken cancellationToken = default)
            {
                track.Add((@event.Event, @event.EventSourceId));
                return Task.CompletedTask;
            }
        }

        [Test]
        public void should_not_allow_register_of_same_event_handling_sync_async()
        {
            IFunctionalHandler testSubj = new BrokenHandler();

            var events = new[]
            {
                new Event {Payload = new InterviewCreated(), EventSourceId = Id.g1 }
            };

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                foreach (var ev in events)
                {
                    await testSubj.Handle(ev);
                }
            });
        }

        private class BrokenHandler : IFunctionalHandler,
            IEventHandler<InterviewCreated>,
            IAsyncEventHandler<InterviewCreated>,
            IEventHandler<InterviewHardDeleted>
        {
            public Task SaveStateAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Handle(PublishedEvent<InterviewCreated> @event)
            {

            }

            public Task Handle(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Handle(PublishedEvent<InterviewHardDeleted> @event)
            {
                
            }
        }
    }
}