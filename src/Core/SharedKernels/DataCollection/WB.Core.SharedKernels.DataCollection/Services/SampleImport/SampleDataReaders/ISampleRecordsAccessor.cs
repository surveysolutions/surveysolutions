using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Services.SampleImport.SampleDataReaders
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}