namespace WB.Services.Export
{
    internal class WeightedProgress
    {
        public ProgressState? LastReportedProgress { get; set; }
        public double ProgressWeight { get; set; }
    }
}
