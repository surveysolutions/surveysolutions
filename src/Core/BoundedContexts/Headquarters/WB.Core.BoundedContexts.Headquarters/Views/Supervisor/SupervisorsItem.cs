using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Supervisor
{
    public class SupervisorsItem
    {
        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }

        public DateTime CreationDate { get; set; }

        public string Email { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public bool IsArchived { get; set; }
    }
}
