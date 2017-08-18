using System;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class QuantityByResponsibleReportRow
    {
        public QuantityByResponsibleReportRow(Guid responsibleId, long[] periods, string responsibleName, int total)
        {
            this.ResponsibleId = responsibleId;
            this.QuantityByPeriod = periods;
            this.ResponsibleName = responsibleName;
            this.Average = periods.Length > 0 ? Math.Round(periods.Average(), 2) : 0;
            this.Total = total;
        }

        public Guid ResponsibleId { get;  set; }
        public string ResponsibleName { get;  set; }
        public long[] QuantityByPeriod { get;  set; }
        public double Average { get;  set; }
        public long Total { get; set; }
    }
}