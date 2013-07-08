using Main.Core.Entities.SubEntities.Complete;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SubGroupColumnItem : PropagatedGroupColumnItem
    {
        public SubGroupColumnItem(ICompleteGroup group)
        {
            this.ItemPublicKey = group.PublicKey;
            this.Title = group.Title;
        }
    }
}
