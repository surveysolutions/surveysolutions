using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class SpeedByResponsibleReportRow
    {
        public SpeedByResponsibleReportRow(Guid responsibleId, double?[] periods, string responsibleName, double? average, double? total)
        {
            this.ResponsibleId = responsibleId;
            this.SpeedByPeriod = periods;
            this.ResponsibleName = responsibleName;
            this.Average = average;
            this.Total = total;
        }

        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public double?[] SpeedByPeriod { get; set; }
        public double? Average { get; set; }
        public double? Total { get; set; }
    }
}
