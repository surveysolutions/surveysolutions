using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}