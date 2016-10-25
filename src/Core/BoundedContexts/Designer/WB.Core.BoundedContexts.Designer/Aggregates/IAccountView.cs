using System;
using System.Collections.Generic;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public interface IAccountView : IMembershipAccount, IUserWithRoles
    {
        string UserId { get; set; }
        ISet<SimpleRoleEnum> SimpleRoles { get; set; }
    }
}