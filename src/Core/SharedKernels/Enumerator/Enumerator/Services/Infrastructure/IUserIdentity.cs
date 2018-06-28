using System;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IUserIdentity
    {
        string Name { get; }

        [Obsolete("Should be removed when there is no users of 5.18 in the wild")]
        string Password { get; }
        string PasswordHash { get; }

        string Token { get; }
        Guid UserId { get; }

        string Email { get; }
    }
}
