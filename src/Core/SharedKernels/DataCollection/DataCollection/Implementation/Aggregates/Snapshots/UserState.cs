using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots
{
    public class UserState
    {
        public UserState()
        {
            UserRoles = new UserRoles[0];
        }

        public bool IsUserLockedBySupervisor { get; set; }
        public bool IsUserLockedByHQ { get; set; }
        public bool IsUserArchived { get; set; }
        public UserRoles[] UserRoles  { get; set; }
        public Guid UserSupervisorId { get; set; }
        public string LoginName { get; set; }
    }
}