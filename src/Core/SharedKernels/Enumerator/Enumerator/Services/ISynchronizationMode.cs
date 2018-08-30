namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ISynchronizationMode
    {
        void Set(SynchronizationMode mode);
        SynchronizationMode GetMode();
    }

    public enum SynchronizationMode
    {
        Online,
        Offline
    }
}
