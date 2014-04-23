namespace WB.Core.Synchronization
{
    public class SyncSettings
    {
        public SyncSettings(bool reevaluateInterviewWhenSynchronized)
        {
            this.ReevaluateInterviewWhenSynchronized = reevaluateInterviewWhenSynchronized;
        }

        public bool ReevaluateInterviewWhenSynchronized { get; private set; }
    }
}