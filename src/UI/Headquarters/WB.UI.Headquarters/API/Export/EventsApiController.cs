using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    [RoutePrefix("api/export/v1")]
    public class EventsApiController : ApiController
    {
        private readonly IHeadquartersEventStore headquartersEventStore;

        public EventsApiController(IHeadquartersEventStore headquartersEventStore)
        {
            this.headquartersEventStore = headquartersEventStore;
        }

        [Route("interview/events", Name = "EventsFeed")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> Get(int sequence = 0, int pageSize = 500)
        {
            var maximum = await this.headquartersEventStore.GetMaximumGlobalSequence();
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.Content = new PushStreamContent((outputStream, content, context) =>
            {
                using (var sw = new StreamWriter(outputStream))
                using (var json = new JsonTextWriter(sw))
                {
                    json.WriteStartObject();
                    json.WritePropertyName(@"total");
                    json.WriteValue(maximum);

                    json.WritePropertyName(nameof(EventsFeedPage.Events));
                    json.WriteStartArray();

                    var events = headquartersEventStore.GetRawEventsFeed(sequence, pageSize);

                    foreach (var ev in events)
                    {
                        json.WriteStartObject();

                        json.WritePropertyName(@"$type"); json.WriteValue(ev.EventType);

                        json.WritePropertyName(nameof(FeedEvent.GlobalSequence));
                        json.WriteValue(ev.GlobalSequence);
                        json.WritePropertyName(nameof(FeedEvent.EventSourceId));
                        json.WriteValue(ev.EventSourceId);
                        json.WritePropertyName(nameof(FeedEvent.Sequence));
                        json.WriteValue(ev.EventSequence);
                        json.WritePropertyName(nameof(FeedEvent.Payload));
                        json.WriteRawValue(ev.Value);
                        json.WritePropertyName(nameof(FeedEvent.EventTimeStamp));
                        json.WriteValue(ev.TimeStamp);
                        json.WriteEndObject();
                    }

                    json.WriteEndArray();
                    json.WriteEndObject();
                }

            }, MediaTypeHeaderValue.Parse(@"application/json"));

            return response;
        }

        [Route("interview/events/{id:guid}", Name = "InterviewEventsFeed")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> Get(Guid id, int lastSequence = 0, int pageSize = 500)
        {
            var maximum = await this.headquartersEventStore.GetMaximumGlobalSequence();
            var events = this.headquartersEventStore.Read(id, minVersion: lastSequence).Take(pageSize).ToList();

            var eventsFeedPage = new EventsFeedPage
            {
                Total = maximum,
                //NextPageUrl = Url.Route("InterviewEventsFeed", new { lastSequence = events.LastOrDefault() }),
                Events = events.Select(e => new FeedEvent(e)).ToList()
            };

            return Request.CreateResponse(eventsFeedPage);
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
