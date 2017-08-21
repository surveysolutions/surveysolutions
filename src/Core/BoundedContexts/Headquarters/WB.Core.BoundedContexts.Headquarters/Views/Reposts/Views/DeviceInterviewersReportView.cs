using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class DeviceInterviewersReportView : IListView<DeviceInterviewersReportLine>
    {
        public DeviceInterviewersReportView()
        {
            this.Items = new List<DeviceInterviewersReportLine>();
        }

        public int TotalCount { get; set; }
        public IEnumerable<DeviceInterviewersReportLine> Items { get; set; }
        public DeviceInterviewersReportLine TotalRow { get; set; }
    }
}