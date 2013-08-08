using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}