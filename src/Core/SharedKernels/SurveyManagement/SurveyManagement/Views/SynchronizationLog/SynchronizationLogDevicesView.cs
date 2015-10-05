using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog
{
    public class SynchronizationLogDevicesView
    {
        public IEnumerable<string> Devices { get; set; }
        public long TotalCountByQuery { get; set; }
    }
}