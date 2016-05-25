using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog
{
    public class SynchronizationLogDevicesView
    {
        public IEnumerable<string> Devices { get; set; }
        public long TotalCountByQuery { get; set; }
    }
}