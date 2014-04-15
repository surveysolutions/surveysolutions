using System;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Atom
{
    public struct Link
    {
        public string Rel { get; set; }

        public Uri Href { get; set; }
    }
}