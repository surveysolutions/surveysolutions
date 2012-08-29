// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    using System.Collections.Generic;
    using System.Linq;

    using Raven.Client;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The collection item browse view factory.
    /// </summary>
    public class CollectionItemBrowseViewFactory :
        IViewFactory<CollectionItemBrowseInputModel, CollectionItemBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDocumentSession documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CollectionItemBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
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
        /// The RavenQuestionnaire.Core.Views.CollectionItem.CollectionItemBrowseView.
        /// </returns>
        public CollectionItemBrowseView Load(CollectionItemBrowseInputModel input)
        {
            int count = this.documentSession.Query<CollectionDocument>().Count();
            if (count == 0)
            {
                return new CollectionItemBrowseView(
                    input.CollectionId, new List<CollectionItemBrowseItem>(), input.QuestionId);
            }

            var doc = this.documentSession.Load<CollectionDocument>(input.CollectionId);
            IList<CollectionItem> collectionItem = doc.Items;
            if (collectionItem.Count != 0)
            {
                List<CollectionItemBrowseItem> result =
                    collectionItem.Select(item => new CollectionItemBrowseItem(item.Key, item.Value)).ToList();
                return new CollectionItemBrowseView(input.CollectionId, result, input.QuestionId);
            }

            return null;
        }

        #endregion
    }
}