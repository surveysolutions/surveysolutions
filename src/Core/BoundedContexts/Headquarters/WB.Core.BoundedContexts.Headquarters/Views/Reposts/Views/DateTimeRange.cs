using System;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    [DebuggerDisplay("[{From.ToString(\"u\")}, {To.ToString(\"u\")})")]
    public class DateTimeRange
    {
        public DateTimeRange(DateTime @from, DateTime to)
        {
            this.From = @from;
            this.To = to;
        }

        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}