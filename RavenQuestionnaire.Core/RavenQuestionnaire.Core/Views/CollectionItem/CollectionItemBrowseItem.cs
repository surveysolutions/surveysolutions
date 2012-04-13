using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseItem
    {
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }
        private string _id;

        public string Key
        {
            get;
            private set;
        }

        public string Value { get; private set; }

        public CollectionItemBrowseItem(string id, string key, string value)
        {
            this.Id = id;
            this.Key = key;
            this.Value = value;
        }
    }
}
