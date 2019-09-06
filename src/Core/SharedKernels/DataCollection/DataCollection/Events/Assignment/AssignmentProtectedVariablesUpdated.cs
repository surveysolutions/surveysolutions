using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentProtectedVariablesUpdated : AssignmentEvent
    {
        public List<string> ProtectedVariables { get; }

        public AssignmentProtectedVariablesUpdated(Guid userId, DateTimeOffset originDate, List<string> protectedVariables) 
            : base(userId, originDate)
        {
            ProtectedVariables = protectedVariables;
        }
    }
}
