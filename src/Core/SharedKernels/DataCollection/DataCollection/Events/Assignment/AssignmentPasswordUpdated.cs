using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentPasswordUpdated : AssignmentEvent
    {
        public string Password { get; }

        public AssignmentPasswordUpdated(Guid userId, DateTimeOffset originDate, string password) 
            : base(userId, originDate)
        {
            Password = password;
        }
    }
}
