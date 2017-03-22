using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership
{
    public interface IMembershipWebUser
    {
        DesignerMembershipUser MembershipUser { get; }

        Guid UserId { get; }

        string UserName { get; }

        bool IsAdmin { get; }

        bool CanImportOnHq { get; }
    }
}