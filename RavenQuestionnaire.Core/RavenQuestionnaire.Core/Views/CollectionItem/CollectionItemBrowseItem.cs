namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseItem
    {
        public string Key
        {
            get;
            private set;
        }

        public string Value { get; private set; }

        public CollectionItemBrowseItem(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
