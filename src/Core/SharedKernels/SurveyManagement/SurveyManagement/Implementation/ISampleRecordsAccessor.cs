using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}