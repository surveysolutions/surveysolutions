// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRoles.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The user roles.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    /// <summary>
    /// The user roles.
    /// </summary>
    public enum UserRoles
    {
        /// <summary>
        /// The administrator.
        /// </summary>
        Administrator, 

        /// <summary>
        /// The supervisor.
        /// </summary>
        Supervisor, 

        /// <summary>
        /// The manager.
        /// </summary>
        Manager, 

        /// <summary>
        /// The operator.
        /// </summary>
        Operator, 

        /// <summary>
        /// The user.
        /// </summary>
        User
    }
}