// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupNestedContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Group;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PropagatedGroupNestedContainer : PropagatedGroupContainer<PropagatedGroupMobileView>
    {
        public PropagatedGroupNestedContainer(CompleteQuestionnaireStoreDocument doc, ICompleteGroup @group) : base(doc, @group)
        {
            this.PropagateTemplate = new PropagatedGroupMobileView(doc, @group);
        }
        public void AddRow(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            base.AddRow(new PropagatedGroupMobileView(doc, group));
        }
        /// <summary>
        /// Gets or sets the propagate template.
        /// </summary>
        public PropagatedGroupMobileView PropagateTemplate { get; set; }
    }
}
