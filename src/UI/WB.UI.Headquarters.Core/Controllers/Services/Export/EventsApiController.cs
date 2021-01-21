using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    [Route("api/export/v1")]
    [Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class EventsApiController : Controller
    {
        private readonly IHeadquartersEventStore headquartersEventStore;
        private readonly ILogger<EventsApiController> logger;

        public EventsApiController(IHeadquartersEventStore headquartersEventStore,
            ILogger<EventsApiController> logger)
        {
            this.headquartersEventStore = headquartersEventStore;
            this.logger = logger;
        }

        [Route("interview/events", Name = "EventsFeed")]
        [HttpGet]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Get(int sequence = 0, int pageSize = 500)
        {
            var maximum = await this.headquartersEventStore.GetMaximumGlobalSequence();

            var timer = Stopwatch.StartNew();
            var ms = new MemoryStream();
            long eventsCount = 0;
            await using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                using var json = new JsonTextWriter(sw);

                await json.WriteStartObjectAsync();
                await json.WritePropertyNameAsync(@"total");
                await json.WriteValueAsync(maximum);

                await json.WritePropertyNameAsync(nameof(EventsFeedPage.Events));
                await json.WriteStartArrayAsync();

                var events = headquartersEventStore.GetRawEventsFeed(sequence, pageSize, 400 * 1024 * 1024 /* 400 Mb */);

                // writing events in stream "as is" without serialization/deserialization
                // to reduce memory pressure in HQ writing JSON manually
                foreach (var ev in events)
                {
                    await json.WriteStartObjectAsync();

                    await json.WritePropertyNameAsync(@"$type");
                    await json.WriteValueAsync(ev.EventType);

                    await json.WritePropertyNameAsync(nameof(FeedEvent.GlobalSequence));
                    await json.WriteValueAsync(ev.GlobalSequence);
                    await json.WritePropertyNameAsync(nameof(FeedEvent.EventSourceId));
                    await json.WriteValueAsync(ev.EventSourceId);
                    await json.WritePropertyNameAsync(nameof(FeedEvent.Sequence));
                    await json.WriteValueAsync(ev.EventSequence);
                    await json.WritePropertyNameAsync(nameof(FeedEvent.Payload));
                    await json.WriteRawValueAsync(ev.Value); // writing event payload 
                    await json.WritePropertyNameAsync(nameof(FeedEvent.EventTimeStamp));
                    await json.WriteValueAsync(ev.TimeStamp);
                    await json.WriteEndObjectAsync();
                    eventsCount++;
                }

                await json.WriteEndArrayAsync();
                await json.WriteEndObjectAsync();
            }

            ms.Position = 0;
            this.logger.LogInformation("Return {eventsCount} events in {elapsed} with {bytes} size",
                eventsCount,
                timer.Elapsed,
                ms.Length.Bytes());
            return File(ms, "application/json");
        }

        [Route("interview/events/{id:guid}", Name = "InterviewEventsFeed")]
        [HttpGet]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<EventsFeedPage>> Get(Guid id, int lastSequence = 0, int pageSize = 500)
        {
            var maximum = await this.headquartersEventStore.GetMaximumGlobalSequence();
            var events = this.headquartersEventStore.Read(id, minVersion: lastSequence).Take(pageSize).ToList();

            var eventsFeedPage = new EventsFeedPage
            {
                Total = maximum,
                Events = events.Select(e => new FeedEvent(e)).ToList()
            };

            return eventsFeedPage;
        }
    }

    public class EventsFeedPage
    {
        public long NextSequence { get; set; }
        public long Total { get; set; }
        public List<FeedEvent> Events { get; set; }
    }

    public class FeedEvent
    {
        public FeedEvent(CommittedEvent committedEvent)
        {
            Sequence = committedEvent.EventSequence;
            EventSourceId = committedEvent.EventSourceId;
            GlobalSequence = committedEvent.GlobalSequence;
            Payload = committedEvent.Payload;
            EventTypeName = committedEvent.Payload.GetType().Name;
            EventTimeStamp = committedEvent.EventTimeStamp;
        }

        public DateTime EventTimeStamp { get; set; }
        public string EventTypeName { get; set; }
        public int Sequence { get; set; }
        public Guid EventSourceId { get; set; }
        public long GlobalSequence { get; set; }
        public Object Payload { get; set; }
    }
}
