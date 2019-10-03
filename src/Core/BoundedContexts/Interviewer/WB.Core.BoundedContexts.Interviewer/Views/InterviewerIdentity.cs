using System;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerIdentity : IInterviewerUserIdentity, IPlainStorageEntity
    {
        public string Name { get;  set; }

        /// <summary>
        /// Updated password hash
        /// </summary>
        public string PasswordHash { get; set; }

        public Guid UserId { get; set; }

        public Guid SupervisorId { get; set; }

        public string Token { get; set; }

        [PrimaryKey]
        public string Id { get; set; }

        public string Email { get; set; }

        public string SecurityStamp { get; set; }

        public string TenantId { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastUpdated { get; set; }

        public bool? IsDeleted { get; set; } 
    }
}
