using System;

namespace WB.Core.SharedKernels.DataCollection.Events.User
{
    [Serializable]
    public class UserLockedBySupervisor 
    {
        public Guid UserId { get; private set; }

        public UserLockedBySupervisor(Guid userId)
        {
            this.UserId = userId;
        }
    }
}