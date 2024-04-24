using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

public interface IInterviewTreeCriticalityRunner
{
    IEnumerable<Tuple<Guid, bool>> RunCriticalRules();
}
