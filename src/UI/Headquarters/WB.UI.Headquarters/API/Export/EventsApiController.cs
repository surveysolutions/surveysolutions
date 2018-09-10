using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    public class EventsApiController : ApiController
    {
        private readonly IHeadquartersEventStore headquartersEventStore;

        public EventsApiController(IHeadquartersEventStore headquartersEventStore)
        {
            this.headquartersEventStore = headquartersEventStore;
        }

        [Route("api/v1/interview/events", Name = "EventsFeed")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(int globalSequence = 0, int pageSize = 500)
        {
            var events = await headquartersEventStore.GetEventsFeedAsync(globalSequence, pageSize);

            var eventsFeedPage = new EventsFeedPage
            {
                NextPageUrl = Url.Route("EventsFeed", new {startWith = events.CurrentGlobalSequenceValue}),
                Events = events.Events.Select(e => new FeedEvent(e)).ToList()
            };

            return Request.CreateResponse(eventsFeedPage);
        }

        [Route("api/v1/interview/events/{id:guid}", Name = "InterviewEventsFeed")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage Get(Guid id, int lastSequence = 0, int pageSize = 500)
        {
            var events = this.headquartersEventStore.Read(id, minVersion: lastSequence).Take(pageSize).ToList();

            var eventsFeedPage = new EventsFeedPage
            {
                NextPageUrl = Url.Route("InterviewEventsFeed", new {lastSequence = events.LastOrDefault()}),
                Events = events.Select(e => new FeedEvent(e)).ToList()
            };

            return Request.CreateResponse(eventsFeedPage);
        }
    }

    public class EventsFeedPage
    {
        /// <summary>
        /// Relative url for fetching next batch of events. Null if application retrieved last page
        /// </summary>
        public string NextPageUrl { get; set; }

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
        }

        public string EventTypeName { get; set; }

        public int Sequence { get; set; }

        public Guid EventSourceId { get; set; }

        public long GlobalSequence { get; set; }

        public Object Payload { get; set; }
    }
}
