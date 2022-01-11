using System;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserToValidateWorkspace
    {
        public string WorkspaceName { get; set; }
        public Guid?  SupervisorId { get; set; }
    }
}