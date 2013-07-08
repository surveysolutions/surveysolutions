using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
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
    public class PropagatedGroupGridContainer
    {
        public PropagatedGroupGridContainer(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            this.PublicKey = group.PublicKey;
            this.Enabled = group.Enabled;
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Title = group.Title;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            this.Row = new List<PropagatedGroupRowItem>();
            this.Description = group.Description;
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
            if (!group.PropagationPublicKey.HasValue)
                throw new ArgumentException("group have to be propagated");
            AddRow(new PropagatedGroupRowItem(group, doc.GetGroupTitle(group.PropagationPublicKey.Value)));
        }

        public void AddRow(PropagatedGroupRowItem row)
        {
            this.Row.Add(row);
        }

        public string Description { get; set; }
        public bool Enabled { get; set; }
        public Guid QuestionnairePublicKey { get; set; }
        public List<PropagatedGroupRowItem> Row { get; set; }

        /// <summary>
        /// Gets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; private set; }

        #region Implementation of ICompositeView

        public Guid PublicKey { get; set; }
        public string Title { get; set; }

        #endregion

        public List<PropagatedGroupColumnItem> Columns { get; set; }
    }
}
