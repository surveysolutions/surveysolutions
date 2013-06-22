// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryView.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryView
    {
        #region Public Properties
        
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public IEnumerable<SummaryViewItem> Items { get; set; }

        #endregion
    }
}