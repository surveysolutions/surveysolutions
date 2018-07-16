namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISyncBgService<T>
    {
        void StartSync();

        T CurrentProgress { get; }
    }
}
