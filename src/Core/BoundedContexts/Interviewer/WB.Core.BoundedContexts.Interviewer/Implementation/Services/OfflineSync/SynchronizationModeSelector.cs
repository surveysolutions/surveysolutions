namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync
{
    public class SynchronizationModeSelector : ISynchronizationMode
    {
        private SynchronizationMode mode;

        public SynchronizationMode GetMode()
        {
            return this.mode;
        }

        public void Set(SynchronizationMode mode)
        {
            this.mode = mode;
        }
    }
}
