using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserToValidate
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsArchived { get; set; }
        public bool IsSupervisor { get; set; }
        public bool IsInterviewer { get; set; }
        public List<UserToValidateWorkspace> Workspaces { get; set; }
    }
}
