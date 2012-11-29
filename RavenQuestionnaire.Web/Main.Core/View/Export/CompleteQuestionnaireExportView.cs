// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Core.View.Export
{
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
            this.Header = new Dictionary<Guid, HeaderItem>();
            this.AutoPropagatebleQuestionsPublicKeys=new List<Guid>();
        }
       /* public CompleteQuestionnaireExportView(string title, IEnumerable<CompleteQuestionnaireExportItem> items, IEnumerable<Guid> subGroups, Dictionary<Guid, HeaderItem> header, List<Guid> autoQuestions)
        {
            this.Items = items;
            this.SubPropagatebleGroups = subGroups;
            this.Header = header;
            this.AutoPropagatebleQuestionsPublicKeys = autoQuestions;
            this.GroupName = title;
        }*/

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
        public CompleteQuestionnaireExportView(Guid publicKey, Guid? parent, string title, IEnumerable<CompleteQuestionnaireExportItem> items, IEnumerable<Guid> subPropagatebleGroups, IEnumerable<Guid> autoQuestions, Dictionary<Guid, HeaderItem> header)
        {
            this.PublicKey = publicKey;
            this.Parent = parent;
            this.GroupName = title;
            this.Items = items;
            this.Header = header;
            this.SubPropagatebleGroups = subPropagatebleGroups;
            this.AutoPropagatebleQuestionsPublicKeys = autoQuestions;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<CompleteQuestionnaireExportItem> Items { get; private set; }

        public IEnumerable<Guid> SubPropagatebleGroups { get; private set; }

        public Dictionary<Guid, HeaderItem> Header { get; private set; }

        public string GroupName { get; private set; }

        public IEnumerable<Guid> AutoPropagatebleQuestionsPublicKeys { get; private set; }

        public Guid? Parent { get; private set; }
        public Guid PublicKey { get; private set; }
        #endregion

        public CompleteQuestionnaireExportView Merge(CompleteQuestionnaireExportView view)
        {

            List<CompleteQuestionnaireExportItem> items = new List<CompleteQuestionnaireExportItem>(this.Items);
            int i = 0;
            foreach (CompleteQuestionnaireExportItem completeQuestionnaireExportItem in view.Items)
            {
                foreach (KeyValuePair<Guid, string> value in completeQuestionnaireExportItem.Values)
                {
                    items[i].Values.Add(value.Key, value.Value);
                }
                i++;
            }

            List<Guid> subgroups = new List<Guid>(this.SubPropagatebleGroups);
            subgroups.AddRange(view.SubPropagatebleGroups);
            subgroups = subgroups.Distinct().ToList();

            Dictionary<Guid, HeaderItem> header = new Dictionary<Guid, HeaderItem>(this.Header);
            foreach (KeyValuePair<Guid, HeaderItem> headerITem in view.Header)
            {
                header.Add(headerITem.Key, headerITem.Value);
            }

            List<Guid> autoQuestions = new List<Guid>(this.AutoPropagatebleQuestionsPublicKeys);
            autoQuestions.AddRange(view.AutoPropagatebleQuestionsPublicKeys);
            autoQuestions = subgroups.Distinct().ToList();
            var result = new CompleteQuestionnaireExportView(this.PublicKey, this.Parent, this.GroupName, items, subgroups, autoQuestions, header);
            return result;
        }
    }
}