using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentsService : IAssignmentsService
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsAccessor;

        public AssignmentsService(IPlainStorageAccessor<Assignment> assignmentsAccessor)
        {
            this.assignmentsAccessor = assignmentsAccessor;
        }

        public List<Assignment> GetAssignments(Guid responsibleId)
        {
            return this.assignmentsAccessor.Query(x =>
            x.Where(assigment =>
                assigment.ResponsibleId == responsibleId
                && !assigment.Archived
                && (assigment.Capacity == null || assigment.InterviewSummaries.Count < assigment.Capacity))
            .ToList());
        }
    }
}