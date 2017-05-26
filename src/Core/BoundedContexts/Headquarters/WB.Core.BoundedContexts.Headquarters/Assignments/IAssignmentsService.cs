using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsService
    {
        List<Assignment> GetAssignments(Guid responsibleId);
    }
}