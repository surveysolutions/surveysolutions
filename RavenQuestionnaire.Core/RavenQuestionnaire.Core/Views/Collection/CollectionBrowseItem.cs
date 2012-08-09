namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionBrowseItem
    {
        public string Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        private string _id;

        public string Name
        {
            get;
            private set;
        }
        
        public CollectionBrowseItem(string id, string title)
        {
            this.Id = id;
            this.Name = title;
        }
    }
}
