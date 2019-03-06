using System;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface IIdentityService
    {
        IIdentityUser GetById(string id);
        IIdentityUser GetByNameOrEmail(string id);
        // todo remove it and replace with GetById
        string GetUserNameByEmail(string email);
    }

    public interface IIdentityUser
    {
        string UserName { get; }
        string Email { get; }
        Guid Id { get; }
    }
}
