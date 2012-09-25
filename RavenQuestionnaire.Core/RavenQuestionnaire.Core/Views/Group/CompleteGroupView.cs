// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.Question;

    /// <summary>
    /// The complete group view.
    /// </summary>
    public class CompleteGroupView :
        GroupView<CompleteGroupView, CompleteQuestionView, ICompleteGroup, ICompleteQuestion>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupView"/> class.
        /// </summary>
        public CompleteGroupView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupView"/> class.
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
        public CompleteGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ICompleteGroupFactory groupFactory)
            : base(doc, group)
        {
            this.Description = doc.Description;
            this.ConditionExpression = doc.ConditionExpression;
            this.Questions =
                group.Children.OfType<ICompleteQuestion>().Select(q => new CompleteQuestionView(doc, q)).ToArray();
            this.Groups =
                group.Children.OfType<ICompleteGroup>().Select(g => CreateGroup(doc, g, groupFactory)).ToArray();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create group.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="groupFactory">
        /// The group Factory.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Group.CompleteGroupView.
        /// </returns>
        public static CompleteGroupView CreateGroup(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ICompleteGroupFactory groupFactory)
        {
            //// PropagatableCompleteGroup propagatableGroup = group as PropagatableCompleteGroup;
            if (group.PropogationPublicKey.HasValue)
            {
                return new PropagatableCompleteGroupView(doc, group, groupFactory);
            }

            return new CompleteGroupView(doc, group, groupFactory);
        }

        /// <summary>
        /// The get client id.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }

        #endregion
    }
}