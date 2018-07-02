using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class SupervisorIdentity : ISupervisorUserIdentity, IPlainStorageEntity
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
