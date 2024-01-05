using System.Collections.Generic;
using System.Linq;
using WB.UI.Headquarters.Configs;

namespace WB.UI.Headquarters.Code.UsersManagement;

public class UsersManagementSettings
{
    public UsersManagementSettings(AccountManagementConfig accountManagementConfig)
    {
        if(accountManagementConfig?.RestrictedUser != null)
            RestrictedUsersInLower = new HashSet<string>(accountManagementConfig.RestrictedUser.Select(x => x.ToLowerInvariant()));
    }

    public HashSet<string> RestrictedUsersInLower { get; set; } = new HashSet<string>();
}
