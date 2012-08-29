// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Group
{
    using System.Linq;

    using RavenQuestionnaire.Core.AbstractFactories;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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
            this.ConditionExpression = doc.ConditionExpression;
            this.Questions =
                group.Children.OfType<ICompleteQuestion>().Select(
                    q => new CompleteQuestionFactory().CreateQuestion(doc, q)).ToArray();
            this.Groups =
                group.Children.OfType<ICompleteGroup>().Select(g => groupFactory.CreateGroup(doc, g)).ToArray();
        }

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
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }

        #endregion
    }
}