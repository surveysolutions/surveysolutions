using Designer.Web.Providers.Roles;

namespace Designer.Web.Providers.Repositories.CQRS
{
    public class CQRSRoleRepository : IRoleRepository
    {
        public IUserWithRoles GetUser(string applicationName, string username)
        {
            throw new System.NotImplementedException();
        }

        public void CreateRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public void AddUserToRole(string applicationName, string roleName, string username)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveUserFromRole(string applicationName, string roleName, string username)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<string> GetRoleNames(string applicationName)
        {
            throw new System.NotImplementedException();
        }

        public bool Exists(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public int GetNumberOfUsersInRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<string> FindUsersInRole(string applicationName, string roleName, string userNameToMatch)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<string> GetUsersInRole(string applicationName, string roleName)
        {
            throw new System.NotImplementedException();
        }
    }
}
