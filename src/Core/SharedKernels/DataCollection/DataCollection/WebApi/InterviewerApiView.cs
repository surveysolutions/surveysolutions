using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewerApiView
    {
        public Guid Id { get; set; }
        public Guid SupervisorId { get; set; }

        public string SecurityStamp { get; set; }
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
