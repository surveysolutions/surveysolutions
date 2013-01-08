// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserEventSync.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the IUserEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System.Collections.Generic;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The UserEventSync interface.
    /// </summary>
    public interface IUserEventSync
    {
        /// <summary>
        /// The get users.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<AggregateRootEvent> GetUsers(UserRoles? role);
    }
}
