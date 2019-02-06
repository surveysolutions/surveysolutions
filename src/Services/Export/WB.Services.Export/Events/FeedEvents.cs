using System.Collections.Generic;

namespace WB.Services.Export.Events
{
    public class FeedEvents
    {
        public long Total { get; set; }
        public string NextPageUrl { get; set; }
        public List<FeedEvent> Events { get; set; }
    }
}