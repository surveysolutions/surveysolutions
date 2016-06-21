namespace Ncqrs.Eventing.Storage
{
    public class EventReadingProgress
    {
        public EventReadingProgress(long current, long maximum)
        {
            this.Current = current;
            this.Maximum = maximum;
        }

        public long Current { get; }
        public long Maximum { get; }
    }
}