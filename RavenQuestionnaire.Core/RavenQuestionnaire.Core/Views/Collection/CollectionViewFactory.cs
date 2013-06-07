// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;

namespace RavenQuestionnaire.Core.Views.Collection
{
    using System.Linq;

    using Main.Core.Documents;

    /// <summary>
    /// The collection view factory.
    /// </summary>
    public class CollectionViewFactory : IViewFactory<CollectionViewInputModel, CollectionView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CollectionDocument> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public CollectionViewFactory(IQueryableDenormalizerStorage<CollectionDocument> documentItemSession)
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
        /// The RavenQuestionnaire.Core.Views.Collection.CollectionView.
        /// </returns>
        public CollectionView Load(CollectionViewInputModel input)
        {
            CollectionDocument doc = this.documentItemSession.Query().FirstOrDefault(u => u.Id == input.CollectionId);
            return new CollectionView(doc);
        }

        #endregion
    }
}