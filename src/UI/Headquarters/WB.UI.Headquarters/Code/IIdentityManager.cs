namespace WB.UI.Headquarters.Code
{
    public interface IIdentityManager
    {
        string[] GetUsersInRole(string roleName);
        string[] GetRolesForUser(string userName);
    }
}