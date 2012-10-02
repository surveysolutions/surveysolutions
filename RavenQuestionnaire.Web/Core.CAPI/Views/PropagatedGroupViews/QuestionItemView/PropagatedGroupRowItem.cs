// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupRowItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PropagatedGroupRowItem
    {
        public PropagatedGroupRowItem(ICompleteGroup group, string title)
        {
            if (!group.PropogationPublicKey.HasValue)
                throw new ArgumentException("group is not propagateble");
            this.PropagationKey = group.PropogationPublicKey.Value;
            this.Title = title;
            this.Answers = new Dictionary<string, QuestionCellItem>();
            this.Enabled = group.Enabled;
            foreach (ICompleteQuestion question in group.Children.OfType<ICompleteQuestion>())
            {
                this.Answers.Add(question.PublicKey.ToString(), new QuestionCellItem(question));
            }
        }

        public Guid PropagationKey { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
        public Dictionary<string, QuestionCellItem> Answers { get; set; }
    }
}
