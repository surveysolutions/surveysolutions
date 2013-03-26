using System.Collections.Generic;

namespace WB.UI.Designer.Providers.Roles
{
    /// <summary>
    /// Repository used to find users and their roles.
    /// </summary>
    /// <remarks>The application name parameter can safely be ignored if you
    /// only have users/roles for one application in your database table. For more information
    /// read about the role provider in MSDN.</remarks>
    public interface IRoleRepository
    {
        /// <summary>
        /// Get a user
        /// </summary>
        /// <param name="applicationName">Application that the request is for.</param>
        /// <param name="username">Account user name</param>
        /// <returns>User if found; otherwise null.</returns>
        IUserWithRoles GetUser(string applicationName, string username);

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="applicationName">Application that the request is for.</param>
        /// <param name="roleName">Name of role</param>
        void CreateRole(string applicationName, string roleName);

        /// <summary>
        /// Remove a role
        /// </summary>
        /// <param name="applicationName">Application that the request is for.</param>
        /// <param name="roleName">Role to remove</param>
        void RemoveRole(string applicationName, string roleName);

        /// <summary>
        /// Add a user to an existing role
        /// </summary>
        /// <param name="applicationName">Application that the request is for.</param>
        /// <param name="roleName">Role that the user is going to be added to</param>
        /// <param name="username">User name</param>
        void AddUserToRole(string applicationName, string roleName, string username);

        /// <summary>
        /// Remove an user from a role.
        /// </summary>
        /// <param name="applicationName">Application that the request is for.</param>
        /// <param name="roleName">Role that the user is going to be removed from.</param>
        /// <param name="username">User to remove</param>
        void RemoveUserFromRole(string applicationName, string roleName, string username);


        /// <summary>
        /// Gets the role names.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>A list with role names</returns>
        IEnumerable<string> GetRoleNames(string applicationName);


        /// <summary>
        /// Checks if a role exists in the specified application
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>true if found; otherwise false.</returns>
        bool Exists(string applicationName, string roleName);

        /// <summary>
        /// Get number of users in a role
        /// </summary>
        /// <param name="applicationName">Application to check for</param>
        /// <param name="roleName">Name of role</param>
        /// <returns>Number of users</returns>
        int GetNumberOfUsersInRole(string applicationName, string roleName);


        /// <summary>
        /// Finds the users in a role.
        /// </summary>
        /// <param name="applicationName">Application to look in</param>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="userNameToMatch">The user name to match.</param>
        /// <returns>A list of user names.</returns>
        IEnumerable<string> FindUsersInRole(string applicationName, string roleName, string userNameToMatch);

        /// <summary>
        /// Finds the users in a role.
        /// </summary>
        /// <param name="applicationName">Application to look in</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>A list of user names.</returns>
        IEnumerable<string> GetUsersInRole(string applicationName, string roleName);
    }
}