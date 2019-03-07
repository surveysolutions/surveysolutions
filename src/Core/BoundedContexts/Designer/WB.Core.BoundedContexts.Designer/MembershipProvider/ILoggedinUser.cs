using System;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface ILoggedInUser
    {
        Guid Id { get; }
        string Login { get; }
        bool IsAdmin { get; }
        string Email { get; }
    }
}
