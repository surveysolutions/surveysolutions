// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupHeaderItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems
{
    public abstract class PropagatedGroupColumnItem
    {
        public Guid ItemPublicKey { get; set; }
        public string Title { get; set; }
    }
}
