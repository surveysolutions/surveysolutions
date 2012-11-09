// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartDataModel.cs" company="WorldBank">
//   2012
// </copyright>
// <summary>
//   Class for display visual chart
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for display visual chart
    /// </summary>
    public class ChartDataModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataModel"/> class.
        /// </summary>
        public ChartDataModel()
        {
            this.Data = new Dictionary<string, int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataModel"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        public ChartDataModel(string title)
            : this()
        {
            this.Title = title;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Data.
        /// </summary>
        public Dictionary<string, int> Data { get; set; }

        #endregion
    }
}