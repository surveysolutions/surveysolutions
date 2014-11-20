using System;

namespace Ncqrs.Eventing.Sourcing
{
    public class EventAppliedEventArgs : EventArgs
    {
        private readonly UncommittedEvent _event;

        public EventAppliedEventArgs(UncommittedEvent evnt)
        {
            _event = evnt;
        }

        public UncommittedEvent Event
        {
            get { return _event; }
        }
    }
}