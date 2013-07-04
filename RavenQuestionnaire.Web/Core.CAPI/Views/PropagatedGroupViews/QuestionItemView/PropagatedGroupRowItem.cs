using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PropagatedGroupRowItem : ICompositeView
    {
        public PropagatedGroupRowItem(ICompleteGroup group, string title)
        {
            if (!group.PropagationPublicKey.HasValue)
                throw new ArgumentException("group is not propagateble");
            this.PropagationKey = group.PropagationPublicKey.Value;
            this.Title = title;
            this.Answers = new Dictionary<string, QuestionCellItem>();
            this.Enabled = group.Enabled;
            this.PublicKey = group.PublicKey;
            foreach (ICompleteQuestion question in group.Children.OfType<ICompleteQuestion>())
            {
                this.Answers.Add(question.PublicKey.ToString(), new QuestionCellItem(question));
            }
        }

       
        public List<ICompositeView> Children { get; set; }
        public Guid? Parent { get; set; }
        public Guid PublicKey { get; set; }

        public Guid PropagationKey { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
        public Dictionary<string, QuestionCellItem> Answers { get; set; }
    }
}
