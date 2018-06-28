using System;
using Newtonsoft.Json;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerIdentity : IInterviewerUserIdentity, IPlainStorageEntity
    {
        public string Name { get;  set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        [Obsolete("Should be removed after 5.19 release of Interviewer")]
        [JsonIgnore]
        public string Password { get; set; }

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
    }
}
