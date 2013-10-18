using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure
{
    public class EventHandlerDescription
    {
        public EventHandlerDescription(string name,string[] usesViews, string[] buildsViews)
        {
            Name = name;
            UsesViews = usesViews;
            BuildsViews = buildsViews;
        }

        public string Name { get; private set; }
        public string[] UsesViews { get; private set; }
        public string[] BuildsViews { get; private set; }
    }
}
