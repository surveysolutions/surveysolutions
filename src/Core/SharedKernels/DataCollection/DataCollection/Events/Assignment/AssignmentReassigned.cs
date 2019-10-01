using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentReassigned : AssignmentEvent
    {
        public Guid ResponsibleId { get; }
        public string Comment { get; }

        public AssignmentReassigned(Guid userId, DateTimeOffset originDate, Guid responsibleId, string comment) : base(userId, originDate)
        {
            ResponsibleId = responsibleId;
            Comment = comment;
        }
    }
}
