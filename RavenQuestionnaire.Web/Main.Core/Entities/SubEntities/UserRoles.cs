// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRoles.cs" company="">
//   
// </copyright>
// <summary>
//   The user roles.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The user roles.
    /// </summary>
    public enum UserRoles
    {
        /// <summary>
        /// The user.
        /// </summary>
        Undefined =0,

        /// <summary>
        /// The administrator.
        /// </summary>
        Administrator =1, 

        /// <summary>
        /// The supervisor.
        /// </summary>
        Supervisor=2, 

        /// <summary>
        /// The manager.
        /// </summary>
        Manager=3, 

        /// <summary>
        /// The operator.
        /// </summary>
        Operator=4, 

        /// <summary>
        /// The user.
        /// </summary>
        User=5,

        Headquarter=6
    }
}