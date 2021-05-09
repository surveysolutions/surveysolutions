using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;
using IHeadquartersApi = WB.ServicesIntegration.Export.IHeadquartersApi;

namespace WB.Services.Export.Tests.EventsProcessorTests
{
    public class EventsProcessor_Api_Query_Tests : BaseEventsProcessorTest
    {
        private Mock<IHeadquartersApi> apiMock;
        private ServiceProvider provider;

        [SetUp]
        public void Setup()
        {
            // setup
            var services = new ServiceCollection();
           
            this.apiMock = new Mock<IHeadquartersApi>();
            this.provider = Create.SetupEventsProcessor(services, apiMock.Object, true);
        }

        [Test]
        public async Task should_query_max_amount_of_events_queried_for_first_time()
        {
            // arrange
            SetupEvents(apiMock, (0, new EventsFeed
                {
                    Total = 5, // events processor should remember this first value as maximum global sequence
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 1),
                        Create.Event.CreatedInterview(globalSeq: 2),
                        Create.Event.CreatedInterview(globalSeq: 3)
                    }
                }),
                (3, new EventsFeed
                {
                    Total = 10,
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 4),
                        Create.Event.CreatedInterview(globalSeq: 5),
                        Create.Event.CreatedInterview(globalSeq: 6)
                    }
                }),
                (6, new EventsFeed // this feed should not be queried
                {
                    Total = 10,
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 7),
                        Create.Event.CreatedInterview(globalSeq: 8),
                        Create.Event.CreatedInterview(globalSeq: 9)
                    }
                })
            );

            // act
            var processor = provider.GetService<EventsProcessor>();
            await processor.HandleNewEvents(1);

            this.apiMock.Verify(s => s.GetInterviewEvents(0, It.IsAny<int>()), Times.Once);
            this.apiMock.Verify(s => s.GetInterviewEvents(3, It.IsAny<int>()), Times.Once);
            this.apiMock.Verify(s => s.GetInterviewEvents(6, It.IsAny<int>()), Times.Never, 
                "Should not continue query events despite availability");
        }


        [Test]
        public async Task should_stop_query_if_no_events_return()
        {
            // arrange
            SetupEvents(apiMock, (0, new EventsFeed
                {
                    Total = 15, // events processor should remember this first value as maximum global sequence
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 1),
                        Create.Event.CreatedInterview(globalSeq: 2),
                        Create.Event.CreatedInterview(globalSeq: 3)
                    }
                }),
                (3, new EventsFeed
                {
                    Total = 10,
                    Events = new List<Event>()
                })
            );

            // act
            var processor = provider.GetService<EventsProcessor>();
            await processor.HandleNewEvents(1);

            this.apiMock.Verify(s => s.GetInterviewEvents(0, It.IsAny<int>()), Times.Once);
            this.apiMock.Verify(s => s.GetInterviewEvents(3, It.IsAny<int>()), Times.Once);
            this.apiMock.Verify(s => s.GetInterviewEvents(6, It.IsAny<int>()), Times.Never, "Should not continue query events despite availability");
        }

        [Test]
        public void should_propagate_api_reading_exceptions()
        {
            // arrange
            SetupEvents(apiMock, (0, new EventsFeed
                {
                    Total = 15, // events processor should remember this first value as maximum global sequence
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 1),
                        Create.Event.CreatedInterview(globalSeq: 2),
                        Create.Event.CreatedInterview(globalSeq: 3)
                    }
                }),
                (3, new EventsFeed
                {
                    Total = 15,
                    Events = new List<Event>
                    {
                        Create.Event.CreatedInterview(globalSeq: 4),
                        Create.Event.CreatedInterview(globalSeq: 5),
                        Create.Event.CreatedInterview(globalSeq: 6)
                    }
                })
            );

            this.apiMock.Setup(s => s.GetInterviewEvents(6, It.IsAny<int>()))
                .Throws<Exception>();

            // act
            var processor = provider.GetService<EventsProcessor>();

            Assert.ThrowsAsync<Exception>(async () => await processor.HandleNewEvents(1));

            var db = this.provider.GetService<TenantDbContext>();
            var meta = db.GlobalSequence;

            Assert.That(meta.AsLong, Is.EqualTo(6));
        }
    }
}
