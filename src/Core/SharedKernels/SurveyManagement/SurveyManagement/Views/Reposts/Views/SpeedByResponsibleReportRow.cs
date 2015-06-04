using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class SpeedByResponsibleReportRow
    {
        public SpeedByResponsibleReportRow(Guid responsibleId, double?[] periods, string responsibleName, double? average, double? total)
        {
            ResponsibleId = responsibleId;
            SpeedByPeriod = periods;
            ResponsibleName = responsibleName;
            Average = average;
            Total = total;
        }

        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public double?[] SpeedByPeriod { get; set; }
        public double? Average { get; set; }
        public double? Total { get; set; }
    }
}
