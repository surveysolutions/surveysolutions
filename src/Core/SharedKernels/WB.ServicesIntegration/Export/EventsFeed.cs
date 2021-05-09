using System.Collections.Generic;

namespace WB.ServicesIntegration.Export
{
    public class EventsFeed
    {
        public long Total { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
