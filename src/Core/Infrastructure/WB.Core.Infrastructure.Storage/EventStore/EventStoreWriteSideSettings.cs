namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreWriteSideSettings
    {
        public EventStoreWriteSideSettings(int maxCountToRead)
        {
            this.MaxCountToRead = maxCountToRead;
        }

        public int MaxCountToRead { get; private set; }
    }
}
