using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public interface IAccountView : IMembershipAccount, IUserWithRoles
    {
        string UserId { get; set; }
        ISet<SimpleRoleEnum> SimpleRoles { get; set; }
    }
}