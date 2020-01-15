using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsToDeleteFactory
    {
        List<Assignment> LoadBatch(Guid questionnaireId, long questionnaireVersion);
    }
}
