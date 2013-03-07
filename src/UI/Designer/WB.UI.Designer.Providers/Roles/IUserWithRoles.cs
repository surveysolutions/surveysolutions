using System.Collections.Generic;

namespace WB.UI.Designer.Providers.Roles
{
    /// <summary>
    /// A user and it's roles
    /// </summary>
    public interface IUserWithRoles
    {
        /// <summary>
        /// Gets a list of all roles that the user is a member of.
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// Check if the user is a member of the specified role
        /// </summary>
        /// <param name="roleName">Role</param>
        /// <returns>true if user belongs to the role; otherwise false.</returns>
        bool IsInRole(string roleName);
    }
}