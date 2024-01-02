using System.Collections.Generic;
using System.Linq;

namespace WB.UI.Headquarters.Code.UsersManagement;

public class UsersManagementSettings
{
    public UsersManagementSettings(string[] restrictedUsers)
    {
        RestrictedUsersInLower = new HashSet<string>(restrictedUsers.Select(x => x.ToLowerInvariant()));
    }

    public HashSet<string> RestrictedUsersInLower { get; set; }
}
