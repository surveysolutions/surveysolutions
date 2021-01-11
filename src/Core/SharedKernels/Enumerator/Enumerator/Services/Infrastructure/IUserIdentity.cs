using System;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IUserIdentity
    {
        string Id { get; }

        string Name { get; }

        string PasswordHash { get; }

        string Token { get; }

        string TenantId { get; set; }

        Guid UserId { get; }

        string Email { get; }
        
        string Workspace { get; }
    }
}
