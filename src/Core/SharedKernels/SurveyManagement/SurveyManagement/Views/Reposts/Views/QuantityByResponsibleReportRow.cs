using System;
using System.Linq;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class QuantityByResponsibleReportRow
    {
        public QuantityByResponsibleReportRow(Guid responsibleId, long[] periods, string responsibleName, int total)
        {
            ResponsibleId = responsibleId;
            QuantityByPeriod = periods;
            ResponsibleName = responsibleName;
            Average = Math.Round(periods.Average(), 2);
            Total = total;
        }

        public Guid ResponsibleId { get;  set; }
        public string ResponsibleName { get;  set; }
        public long[] QuantityByPeriod { get;  set; }
        public double Average { get;  set; }
        public long Total { get; set; }
    }
}