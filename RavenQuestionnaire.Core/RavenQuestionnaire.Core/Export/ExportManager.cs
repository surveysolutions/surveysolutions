// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportManager.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The export manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

    /// <summary>
    /// The export manager.
    /// </summary>
    public class ExportManager
    {
        #region Fields

        /// <summary>
        /// The _provider.
        /// </summary>
        private readonly IExportProvider _provider;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportManager"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public ExportManager(IExportProvider provider)
        {
            this._provider = provider;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Export(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items, string fileName)
        {
            this._provider.DoExport(template, items, fileName);
            return true;
        }

        /// <summary>
        /// The export to stream.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        public Stream ExportToStream(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items)
        {
            return this._provider.DoExportToStream(template, items);
        }

        #endregion
    }
}