using System;
using System.Web.Security;

namespace WB.UI.Shared.Web.Membership
{
    public interface IMembershipWebUser
    {
        MembershipUser MembershipUser { get; }

        Guid UserId { get; }

        string UserName { get; }

        bool IsAdmin { get; }
    }
}