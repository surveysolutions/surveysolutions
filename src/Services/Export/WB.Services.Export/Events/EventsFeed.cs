﻿using System.Collections.Generic;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsFeed
    {
        public long Total { get; set; }
        public List<Event> Events { get; set; }
    }
}
