// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZipFileData.cs" company="">
//   
// </copyright>
// <summary>
//   The zip file data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Export
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;

    /// <summary>
    /// The zip file data.
    /// </summary>
    public class ZipFileData
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileData"/> class.
        /// </summary>
        public ZipFileData()
        {
            this.ImportDate = DateTime.Now;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the client guid.
        /// </summary>
        public Guid ClientGuid { get; set; }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        public IEnumerable<AggregateRootEvent> Events { get; set; }

        /// <summary>
        /// Gets or sets the import date.
        /// </summary>
        public DateTime ImportDate { get; set; }

        #endregion
    }
}