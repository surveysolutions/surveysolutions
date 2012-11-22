// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System.Collections.Generic;

    /// <summary>
    /// The complete questionnaire export view.
    /// </summary>
    public class CompleteQuestionnaireExportView
    {

        #region Constructors and Destructors
        public CompleteQuestionnaireExportView()
        {
            this.Items = Enumerable.Empty<CompleteQuestionnaireExportItem>();
            this.SubPropagatebleGroups = Enumerable.Empty<Guid>();
            this.Header=new Dictionary<Guid, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportView"/> class.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        public CompleteQuestionnaireExportView(string title, IEnumerable<CompleteQuestionnaireExportItem> items, IEnumerable<Guid> subPropagatebleGroups, Dictionary<Guid, string> header)
        {
            this.GroupName = title;
            this.Items = items;
            this.Header = header;
            this.SubPropagatebleGroups = subPropagatebleGroups;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<CompleteQuestionnaireExportItem> Items { get; private set; }

        public IEnumerable<Guid> SubPropagatebleGroups { get; private set; }

        public Dictionary<Guid, string> Header { get; private set; }

        public string GroupName { get; private set; }

        #endregion
    }
}