// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatedCompleteGroupView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagatable complete group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Group
{
    using System;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The propagatable complete group view.
    /// </summary>
    public class PropagatableCompleteGroupView : CompleteGroupView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatableCompleteGroupView"/> class.
        /// </summary>
        public PropagatableCompleteGroupView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatableCompleteGroupView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="groupFactory">
        /// The group factory.
        /// </param>
        public PropagatableCompleteGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ICompleteGroupFactory groupFactory)
            : base(doc, group, groupFactory)
        {
            this.PropogationPublicKey = group.PropogationPublicKey.Value;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid PropogationPublicKey { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get client id.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}_{2}", prefix, this.PublicKey, this.PropogationPublicKey);
        }

        #endregion
    }
}