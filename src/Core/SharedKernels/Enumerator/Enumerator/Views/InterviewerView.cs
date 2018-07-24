using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class InterviewerDocument : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public Guid InterviewerId { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string FullaName { get; set; }
        public bool IsLockedByHeadquarters { get; set; }
        public bool IsLockedBySupervisor { get; set; }

        public string SecurityStamp { get; set; }
    }
}
