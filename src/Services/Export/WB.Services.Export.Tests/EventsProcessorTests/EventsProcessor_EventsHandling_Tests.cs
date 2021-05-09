using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;
using IHeadquartersApi = WB.ServicesIntegration.Export.IHeadquartersApi;

namespace WB.Services.Export.Tests.EventsProcessorTests
{
    public class EventsProcessor_EventsHandling_Tests : BaseEventsProcessorTest
    {
        [Test]
        public async Task should_handle_only_not_filtered_events()
        {
            var services = new ServiceCollection();
            var apiMock = new Mock<IHeadquartersApi>();
            var handledEvents = new List<PublishedEvent<InterviewCreated>>();

            services.AddTransient<IEventsFilter, SkipSecondSequenceFilter>();
            services.AddTransient<IFunctionalHandler>(c => new EventHandler(handledEvents));

            var provider = Create.SetupEventsProcessor(services, apiMock.Object);

            var processor = provider.GetService<EventsProcessor>();
            SetupEvents(apiMock, (0, new EventsFeed
            {
                Events = new List<Event>
                {
                    Create.Event.CreatedInterview(globalSeq: 1),
                    Create.Event.CreatedInterview(globalSeq: 2),
                    Create.Event.CreatedInterview(globalSeq: 3)
                },
                Total = 3
            }));

            await processor.HandleNewEvents(0);

            Assert.That(handledEvents, Has.Count.EqualTo(2));
            Assert.That(handledEvents.Select(e => e.GlobalSequence), Is.EquivalentTo(new[] { 1, 3 }));
        }

        [Test]
        public void should_log_handle_errors_as_critical()
        {
            var services = new ServiceCollection();
            var apiMock = new Mock<IHeadquartersApi>();
            var handledEvents = new List<PublishedEvent<InterviewCreated>>();
            var logMock = new Mock<ILogger<EventsHandler>>();

            services.AddTransient<IFunctionalHandler>(c => new EventHandler(handledEvents));
            services.AddTransient<ILogger<EventsHandler>>(c => logMock.Object);

            var provider = Create.SetupEventsProcessor(services, apiMock.Object, 
                withDefaultEventsFilter: true, 
                noEventsHandlerLogger: true);

            var processor = provider.GetService<EventsProcessor>();
            SetupEvents(apiMock, (0, new EventsFeed
            {
                Events = new List<Event>
                {
                    Create.Event.CreatedInterview(globalSeq: 1),
                    Create.Event.InterviewOnClientCreated(globalSeq: 3),
                    Create.Event.CreatedInterview(globalSeq: 4)
                },
                Total = 3
            }));

            Assert.ThrowsAsync<ArgumentException>(async () => await processor.HandleNewEvents(0));

            logMock.Verify(LogLevel.Critical, e => e is ArgumentException, Times.Once);
        }

        private class EventHandler : IFunctionalHandler, IEventHandler<InterviewCreated>,
            IEventHandler<InterviewOnClientCreated>
        {
            public EventHandler(List<PublishedEvent<InterviewCreated>> handledEvents)
            {
                this.HandledEvents = handledEvents;
            }

            private List<PublishedEvent<InterviewCreated>> HandledEvents { get; }

            public Task SaveStateAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Handle(PublishedEvent<InterviewCreated> @event)
            {
                HandledEvents.Add(@event);
            }

            public void Handle(PublishedEvent<InterviewOnClientCreated> @event)
            {
                throw new ArgumentException("Cannot handle", nameof(@event));
            }
        }

        private class SkipSecondSequenceFilter : IEventsFilter
        {
            public Task<List<Event>> FilterAsync(ICollection<Event> feed, CancellationToken cancellationToken = default)
            {
                var res = new List<Event>();

                foreach (var @event in feed)
                {
                    if (@event.GlobalSequence != 2)
                        res.Add(@event);
                }

                return Task.FromResult(res);
            }
        }
    }
}
