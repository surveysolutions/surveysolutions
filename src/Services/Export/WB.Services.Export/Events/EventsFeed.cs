using System.Collections.Generic;

namespace WB.Services.Export.Events
{
    public class EventsFeed
    {
        public long Total { get; set; }
        public string NextPageUrl { get; set; }
        public List<Event> Events { get; set; }
    }
}
