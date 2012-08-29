// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntity.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Entity interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    /// <summary>
    /// The Entity interface.
    /// </summary>
    /// <typeparam name="TDoc">
    /// </typeparam>
    public interface IEntity<TDoc>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The TDoc.
        /// </returns>
        TDoc GetInnerDocument();

        #endregion
    }
}