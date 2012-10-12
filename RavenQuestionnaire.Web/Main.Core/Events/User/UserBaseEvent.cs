// -----------------------------------------------------------------------
// <copyright file="UserBaseEvent.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Events.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UserBaseEvent
    {
        /// <summary>
        /// Gets or sets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; set; }

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
