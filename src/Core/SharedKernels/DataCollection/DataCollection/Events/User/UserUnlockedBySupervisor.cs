using System;

namespace WB.Core.SharedKernels.DataCollection.Events.User
{
    [Serializable]
    public class UserUnlockedBySupervisor
    {
        public Guid UserId { get; private set; }

        public UserUnlockedBySupervisor(Guid userId)
        {
            this.UserId = userId;
        }


    }
}