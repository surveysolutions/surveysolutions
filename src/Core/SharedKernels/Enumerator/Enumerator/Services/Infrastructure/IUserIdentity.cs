using System;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IUserIdentity
    {
        string Name { get; }
        string Password { get; }
        Guid UserId { get; }
    }
}