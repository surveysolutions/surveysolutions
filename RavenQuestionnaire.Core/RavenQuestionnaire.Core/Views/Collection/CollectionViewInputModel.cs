namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionViewInputModel
    {
        public string CollectionId { get; set; }

        public CollectionViewInputModel(string collectionId)
        {
            this.CollectionId = collectionId;
        }
    }
}

