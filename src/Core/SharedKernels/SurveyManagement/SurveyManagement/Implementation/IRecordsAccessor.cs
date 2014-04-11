using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation
{
    public interface IRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}