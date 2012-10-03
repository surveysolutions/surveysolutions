// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
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
    public class PropagatedGroupGridContainer : PropagatedGroupContainer<PropagatedGroupRowItem>
    {
        public PropagatedGroupGridContainer(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
            : base(doc, group)
        {
            this.PopulateHeader(group, doc.PublicKey);
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

        public void AddRow(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            if(!group.PropogationPublicKey.HasValue)
                throw new ArgumentException("group have to be propagated");
            base.AddRow(new PropagatedGroupRowItem(group,
                                                   string.Concat(doc.GetPropagatedGroupsByKey(
                                                       group.PropogationPublicKey.Value).
                                                                     SelectMany(q => q.Children).
                                                                     OfType
                                                                     <ICompleteQuestion>().Where(q => q.Capital).Select(
                                                                         q => q.GetAnswerString() + " "))));
        }

        public List<PropagatedGroupColumnItem> Columns { get; set; }
    }



}
