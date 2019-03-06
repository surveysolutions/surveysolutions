using System;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface ILoggedInUser
    {
        Guid Id { get; }
        bool IsAdmin { get; }
    }
}
