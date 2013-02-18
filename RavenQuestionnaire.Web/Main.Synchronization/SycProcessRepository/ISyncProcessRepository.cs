// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncProcessRepository.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SycProcessRepository
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ISyncProcessRepository
    {
        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="synkProcessKey">
        /// The synk process key.
        /// </param>
        /// <returns>
        /// The <see cref="ISyncProcessor"/>.
        /// </returns>
        ISyncProcessor GetProcessor(Guid synkProcessKey);

        #endregion
    }
}