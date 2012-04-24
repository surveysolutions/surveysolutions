using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionViewFactory:IViewFactory<CollectionViewInputModel, CollectionView>
    {
        private IDocumentSession documentSession;

        public CollectionViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CollectionView Load(CollectionViewInputModel input)
        {
            var doc = documentSession.Load<CollectionDocument>(input.CollectionId);
            return new CollectionView(doc);
        }
    }
}