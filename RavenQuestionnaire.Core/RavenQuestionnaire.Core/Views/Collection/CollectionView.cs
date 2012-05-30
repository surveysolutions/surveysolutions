using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.CollectionItem;


namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionView
    {

        public string CollectionId
        {
            get { return IdUtil.ParseId(_collectionId); }
            set { _collectionId = value; }
        }

        private string _collectionId;

        public string Name { get; set; }

        public List<CollectionItemView> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                if (this._items == null)
                {
                    this._items = new List<CollectionItemView>();
                    return;
                }
            }
        }

        private List<CollectionItemView> _items;

        public CollectionView()
        {
            this.Items=new List<CollectionItemView>();
        }

        public CollectionView(ICollectionDocument collection)
        {
            this.CollectionId = collection.Id;
            this.Name = collection.Name;
            CollectionDocument cd = (CollectionDocument)collection;
            this.Items=new List<CollectionItemView>();
            foreach (Entities.SubEntities.CollectionItem item in cd.Items)
                this.Items.Add(new CollectionItemView(cd.Id,
                                                      new Entities.SubEntities.CollectionItem(item.PublicKey, item.Key,
                                                                                              item.Value)));
        }
    }
}

