using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class DateTimeRange
    {
        public DateTimeRange(DateTime @from, DateTime to)
        {
            From = @from;
            To = to;
        }

        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}