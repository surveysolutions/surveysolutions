// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Group
{
    using System;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;

    using Raven.Client;

    /// <summary>
    /// The complete group view factory.
    /// </summary>
    public class CompleteGroupViewFactory : IViewFactory<CompleteGroupViewInputModel, CompleteGroupView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDocumentSession documentSession;

        /// <summary>
        /// The group factory.
        /// </summary>
        private readonly ICompleteGroupFactory groupFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        /// <param name="groupFactory">
        /// The group factory.
        /// </param>
        public CompleteGroupViewFactory(IDocumentSession documentSession, ICompleteGroupFactory groupFactory)
        {
            this.documentSession = documentSession;
            this.groupFactory = groupFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Group.CompleteGroupView.
        /// </returns>
        public CompleteGroupView Load(CompleteGroupViewInputModel input)
        {
            var doc = this.documentSession.Load<CompleteQuestionnaireStoreDocument>(input.QuestionnaireId);
            CompleteGroup group;
            if (input.PublicKey.HasValue)
            {
                group = doc.Find<CompleteGroup>(input.PublicKey.Value);
            }
            else
            {
                group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };
                group.PublicKey = Guid.Empty;
            }

            return CompleteGroupView.CreateGroup(doc, group, this.groupFactory);
        }

        #endregion
    }
}