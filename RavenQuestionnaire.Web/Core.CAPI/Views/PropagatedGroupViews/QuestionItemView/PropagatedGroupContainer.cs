// -----------------------------------------------------------------------
// <copyright file="PropagatedGroupNestedContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;
using Main.Core.View.Group;
using Main.Core.View.Question;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class PropagatedGroupContainer<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatedGroupMobileView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        protected PropagatedGroupContainer(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            this.PublicKey = group.PublicKey;
            this.Enabled = group.Enabled;
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Title = group.Title;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            this.Row = new List<T>();
            this.Description = group.Description;


        }
        public void AddRow(T row)
        {
            this.Row.Add(row);
        }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public Guid QuestionnairePublicKey { get; set; }
        public List<T> Row { get; set; }
        /// <summary>
        /// Gets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; private set; }

        #region Implementation of ICompositeView

        public Guid PublicKey { get; set; }
        public string Title { get; set; }

        #endregion
    }
}
