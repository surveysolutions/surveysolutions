// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;

namespace RavenQuestionnaire.Core.Views.Collection
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;

    /// <summary>
    /// The collection browse view factory.
    /// </summary>
    public class CollectionBrowseViewFactory : IViewFactory<CollectionBrowseInputModel, CollectionBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CollectionDocument> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public CollectionBrowseViewFactory(IQueryableDenormalizerStorage<CollectionDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
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
        /// The RavenQuestionnaire.Core.Views.Collection.CollectionBrowseView.
        /// </returns>
        public CollectionBrowseView Load(CollectionBrowseInputModel input)
        {
            int count = this.documentItemSession.Count();
            if (count == 0)
            {
                return new CollectionBrowseView(input.Page, input.PageSize, count, new CollectionBrowseItem[0]);
            }

            List<CollectionDocument> query =
                this.documentItemSession.Query().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            CollectionBrowseItem[] items = query.Select(x => new CollectionBrowseItem(x.Id, x.Name)).ToArray();
            return new CollectionBrowseView(input.Page, input.PageSize, count, items);
        }

        #endregion
    }
}