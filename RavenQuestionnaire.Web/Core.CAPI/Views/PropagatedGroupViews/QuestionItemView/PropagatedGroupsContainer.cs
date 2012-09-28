// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Complete;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PropagatedGroupsContainer
    {
        public PropagatedGroupsContainer(ICompleteGroup groupTemplate, Guid questionnairePublicKey)
        {
            this.QuestionnairePublicKey = questionnairePublicKey;
            this.GroupPublicKey = groupTemplate.PublicKey;
            this.GroupName = groupTemplate.Title;
            this.PopulateHeader(groupTemplate, questionnairePublicKey);
            this.Row = new List<PropagatedGroupRowItem>();
        }

        protected void PopulateHeader(ICompleteGroup groupTemplate, Guid questionnairePublicKey)
        {
            this.Columns = new List<PropagatedGroupColumnItem>();
            foreach (IComposite composite in groupTemplate.Children)
            {
                var group = composite as ICompleteGroup;
                if (group != null)
                {
                    this.Columns.Add(new SubGroupColumnItem(group));
                    continue;
                }
                var question = composite as ICompleteQuestion;
                if (question != null)
                {
                    this.Columns.Add(new QuestionColumnItem(question, questionnairePublicKey, groupTemplate.PublicKey));
                    continue;

                }
                throw new InvalidCastException(string.Format("unknown group item type {0}", composite.GetType().Name));
            }
        }

        public void AddRow(ICompleteGroup group, string title)
        {
            this.Row.Add(new PropagatedGroupRowItem(group, title));
        }

        public Guid GroupPublicKey { get; set; }
        public string GroupName { get; set; }
        public List<PropagatedGroupColumnItem> Columns { get; set; }
        public List<PropagatedGroupRowItem> Row { get; set; }
        public Guid QuestionnairePublicKey { get; set; }
    }



}
