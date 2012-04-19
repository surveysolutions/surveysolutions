using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionViewInputModel
    {
        public string CollectionId { get; set; }

        public CollectionViewInputModel(string collectionId)
        {
            this.CollectionId = IdUtil.CreateCollectionId(collectionId);
        }
    }
}

