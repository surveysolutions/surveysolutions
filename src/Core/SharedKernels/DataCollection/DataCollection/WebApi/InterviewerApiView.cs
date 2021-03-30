using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewerApiView
    {
        public Guid Id { get; set; }
        
        [Obsolete]
        public Guid SupervisorId { get; set; }
        public string SecurityStamp { get; set; }
        
        public List<UserWorkspaceApiView> Workspaces { get; set; }
    }

    public class WorkspaceApiView
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Disabled { get; set; }
        public string Supervisor { get; set; }
    }
    
    public class UserWorkspaceApiView : WorkspaceApiView
    {
        public Guid? SupervisorId { get; set; }
    }

    public class InterviewerFullApiView
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        
        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHeadquarters { get; set; }
        public string SecurityStamp { get; set; }
    }
}
