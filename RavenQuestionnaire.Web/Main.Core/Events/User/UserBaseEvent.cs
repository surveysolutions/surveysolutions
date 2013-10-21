using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.User
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UserBaseEvent
    {
        public bool IsAssignedRole(UserRoles role)
        {
            return DoCheckIsAssignedRole(role);
        }

        /// <summary>
        /// Check if the event assigns a role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if assigned</returns>
        protected virtual bool DoCheckIsAssignedRole(UserRoles role)
        {
            return false;
        }
    }
}
