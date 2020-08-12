namespace WB.Core.Infrastructure.Metrics
{
    public interface ICounter
    {
        void Inc(double amount = 1);
        ICounter Labels(params string[] labels);
        double Value { get; }
    }
}
