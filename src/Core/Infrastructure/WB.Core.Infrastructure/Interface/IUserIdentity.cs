using System;

namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface IUserIdentity
    {
        string Name { get; }
        string Password { get; }
        Guid UserId { get; }
    }
}