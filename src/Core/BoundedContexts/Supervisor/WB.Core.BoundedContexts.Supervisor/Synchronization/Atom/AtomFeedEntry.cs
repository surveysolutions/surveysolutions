using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Atom
{
    public class AtomFeedEntry<T>
    {
        public string Id { get; set; }
        public DateTime Updated { get; set; }

        public List<Link> Links { get; set; }
        public T Content { get; set; }
    }
}