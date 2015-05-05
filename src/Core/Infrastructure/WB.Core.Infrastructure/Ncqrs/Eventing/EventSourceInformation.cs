using System;

namespace Ncqrs.Eventing
{
    /// <summary>
    /// Contains information about an event source.
    /// </summary>
    public class EventSourceInformation
    {
        private readonly Guid _id;
        private readonly int _initialVersion;
        private readonly int _currentVersion;

        public EventSourceInformation(Guid id, int initialVersion, int currentVersion)
        {
            _id = id;
            _currentVersion = currentVersion;
            _initialVersion = initialVersion;
        }

        public int CurrentVersion
        {
            get { return _currentVersion; }
        }

        public int InitialVersion
        {
            get { return _initialVersion; }
        }

        public Guid Id
        {
            get { return _id; }
        }
    }
}