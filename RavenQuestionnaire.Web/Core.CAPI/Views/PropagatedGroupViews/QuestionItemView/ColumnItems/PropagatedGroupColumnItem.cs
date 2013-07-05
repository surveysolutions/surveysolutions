using System;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems
{
    public abstract class PropagatedGroupColumnItem
    {
        public Guid ItemPublicKey { get; set; }
        public string Title { get; set; }
    }
}
