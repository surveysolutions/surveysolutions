using WB.UI.Headquarters.Resources.Users;

namespace WB.UI.Headquarters.Extensions
{
    public static class LocalizationExtensions
    {
        public static string LocalizedRoleName(this string role)
        {
            return UsersResources.ResourceManager.GetString("ApplicationRole_" + role);
        }
    }
}