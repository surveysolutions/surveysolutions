// -----------------------------------------------------------------------
// <copyright file="ISyncProcessRepository.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.SycProcessRepository
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ISyncProcessRepository
    {
        /// <summary>
        /// </summary>
        /// <param name="synkProcessKey">
        /// The synk process key.
        /// </param>
        /// <returns>
        /// </returns>
        ISyncProcessor GetProcessor(Guid synkProcessKey);
    }
}
