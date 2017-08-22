using System;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class QuantityByResponsibleReportRow
    {
        public QuantityByResponsibleReportRow(int numberOfPeriods)
        {
            this.QuantityByPeriod = new long[numberOfPeriods];
        }

        public Guid ResponsibleId { get;  set; }
        public string ResponsibleName { get;  set; }
        public long[] QuantityByPeriod { get;  set; }

        public double Average => this.QuantityByPeriod.Length > 0 ? Math.Round(this.QuantityByPeriod.Average(), 2) : 0;

        public long Total { get; set; }
    }
}