using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog
{
    public class SynchronizationLog
    {
        public IEnumerable<SynchronizationLogItem> Items { get; set; }
        public long TotalCount { get; set; }
    }
}