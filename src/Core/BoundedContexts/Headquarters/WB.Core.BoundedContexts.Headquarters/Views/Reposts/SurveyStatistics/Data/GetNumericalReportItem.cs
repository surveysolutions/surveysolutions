namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data
{
    /// <summary>
    /// Result of readside.get_numerical_report function
    /// </summary>
    public class GetNumericalReportItem
    {
        public string TeamLeadName { get; set; }
        public string ResponsibleName { get; set; }
        public long Count { get; set; }
        public double Average { get; set; }
        public double Median { get; set; }
        public long Max { get; set; }
        public long Min { get; set; }
        public long Sum { get; set; }
        public double Percentile05 { get; set; }
        public double Percentile50 { get; set; }
        public double Percentile95 { get; set; }
    }
}
