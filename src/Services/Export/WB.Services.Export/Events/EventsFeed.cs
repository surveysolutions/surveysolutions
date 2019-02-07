using System.Collections.Generic;
using Newtonsoft.Json;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsFeed
    {
        public long Total { get; set; }
        public long NextSequence { get; set; }
        public List<Event> Events { get; set; }
    }
}
