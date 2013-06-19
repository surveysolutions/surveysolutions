// -----------------------------------------------------------------------
// <copyright file="StatisticsItemKeysHash.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.DenormalizerStorageItem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StatisticsItemKeysHash : IView
    {
        /// <summary>
        /// Gets or sets StorageGuid.
        /// </summary>
        public Guid StorageKey { get; set; }
    }
}
