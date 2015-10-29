namespace Ncqrs.Eventing.Storage
{
    public struct EventPosition
    {
        public EventPosition(long commitPosition, long preparePosition)
        {
            this.CommitPosition = commitPosition;
            this.PreparePosition = preparePosition;
        }

        public long CommitPosition { get;private set; }
        public long PreparePosition { get; private set; }
    }
}