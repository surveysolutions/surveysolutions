// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IValildationService.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ValildationService interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Services
{
    using System;

    using RavenQuestionnaire.Core.Entities;

    /// <summary>
    /// The ValildationService interface.
    /// </summary>
    public interface IValildationService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        bool Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey);

        #endregion
    }
}