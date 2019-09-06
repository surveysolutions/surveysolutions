using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentReassigned : AssignmentEvent
    {
        public Guid ResponsibleId { get; }

        public AssignmentReassigned(Guid userId, DateTimeOffset originDate, Guid responsibleId) : base(userId, originDate)
        {
            ResponsibleId = responsibleId;
        }
    }
}
