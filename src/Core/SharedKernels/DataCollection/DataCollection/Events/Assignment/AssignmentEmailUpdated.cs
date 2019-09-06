using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentEmailUpdated : AssignmentEvent
    {
        public string Email { get; }

        public AssignmentEmailUpdated(Guid userId, DateTimeOffset originDate, string email) 
            : base(userId, originDate)
        {
            Email = email;
        }
    }
}
