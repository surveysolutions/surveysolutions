using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;

namespace WB.Core.BoundedContexts.Headquarters.Views.SystemLog
{
    public class SystemLog
    {
        public IEnumerable<SystemLogItem> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
