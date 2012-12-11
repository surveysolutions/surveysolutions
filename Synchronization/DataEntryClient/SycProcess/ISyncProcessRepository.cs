// -----------------------------------------------------------------------
// <copyright file="ISyncProcessRepository.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
        ISyncProcessor GetProcess(Guid synkProcessKey);
    }
}
