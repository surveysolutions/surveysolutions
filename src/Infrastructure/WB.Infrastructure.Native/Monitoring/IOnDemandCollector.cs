namespace WB.Infrastructure.Native.Monitoring
{
    public interface IOnDemandCollector
    {
        void RegisterMetrics();
        void UpdateMetrics();
    }
}
