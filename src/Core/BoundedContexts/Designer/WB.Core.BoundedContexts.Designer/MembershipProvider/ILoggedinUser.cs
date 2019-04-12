using System;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface ILoggedInUser
    {
        string Id { get; }
        string Login { get; }
        bool IsAdmin { get; }
    }
}
