using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionViewFactory:IViewFactory<CollectionViewInputModel, CollectionView>
    {
        private readonly IDenormalizerStorage<CollectionDocument> documentItemSession;

        public CollectionViewFactory(IDenormalizerStorage<CollectionDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public CollectionView Load(CollectionViewInputModel input)
        {
            var doc = documentItemSession.Query().FirstOrDefault(u => u.Id == input.CollectionId);
            return new CollectionView(doc);
        }
    }
}