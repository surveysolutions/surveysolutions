using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.CollectionItem;


namespace RavenQuestionnaire.Core.Views.Collection
{
    //public abstract class AbstractCollectionView
    //{

    //    public int Index { get; set; }

    //    public string CollectionId
    //    {
    //        get { return IdUtil.ParseId(_collectionId); }
    //        set { _collectionId = value; }
    //    }

    //    private string _collectionId;

    //    public string Name { get; set; }

    //    protected AbstractCollectionView()
    //    {

    //    }

    //    protected AbstractCollectionView(string collectionId):this()
    //    {
    //        this.CollectionId = collectionId;
    //    }

    //    protected AbstractCollectionView(ICollectionDocument collection):this()
    //    {
    //        this.CollectionId = collection.Id;
    //    }

    //    protected AbstractCollectionView(ICollectionDocument collectionId, ICollectionItem doc):this()
    //    {
    //        this.CollectionId = collectionId.Id;
    //    }
    //}

    //public abstract class AbstractCollectionView<T> : AbstractCollectionView where T : CollectionItemView
    //{

    //    public T[] Items
    //    {
    //        get { return _items; }
    //        set
    //        {
    //            _items = value;
    //            if (this._items == null)
    //            {
    //                this._items = new T[0];
    //                return;
    //            }

    //            for (int i = 0; i < this._items.Length; i++)
    //            {
    //                this._items[i].Index = i + 1;
    //            }

    //        }
    //    }

    //    private T[] _items;

    //    protected AbstractCollectionView():base()
    //    {
    //        Items = new T[0];
    //    }

    //    protected AbstractCollectionView(string collectionId):this()
    //    {
    //        this.CollectionId = collectionId;
    //    }

    //    protected AbstractCollectionView(ICollectionDocument collection):base(collection)
    //    {
    //        var _collection = collection as CollectionDocument;
    //        this.Items = (T[]) _collection.Items.Select(i => new CollectionItemView(collection.Id, new Entities.SubEntities.CollectionItem(i.PublicKey, i.Key, i.Value))).ToArray();
    //        this.Name = collection.Name;
    //    }

    //    protected AbstractCollectionView(ICollectionDocument collection, ICollectionItem doc):base(collection, doc)
    //    {
    //        var _collection = collection as CollectionDocument;
    //        this.Items = (T[]) _collection.Items.Select(i => new CollectionItemView(collection.Id, new Entities.SubEntities.CollectionItem(i.PublicKey, i.Key, i.Value))).ToArray();
    //        this.Name = collection.Name;
    //    }
    //}

    //public abstract class CollectionView<T> : AbstractCollectionView<T>
    //    where T : CollectionItemView
    //{
    //    protected CollectionView()
    //    {
    //    }

    //    protected CollectionView(string collectionId):base(collectionId)
    //    {
    //    }

    //    protected CollectionView(ICollectionDocument collection):base(collection)
    //    {
    //    }

    //    protected CollectionView(ICollectionDocument<ICollectionItem> collection, ICollectionItem doc):base(collection, doc)
    //    {
    //    }

    //}

    //public class CollectionView : CollectionView<CollectionItemView>
    //{
    //    public CollectionView()
    //    {
    //    }

    //    //public CollectionView(string collectionId):base(collectionId)
    //    //{
    //    //}

    //    public CollectionView(ICollectionDocument collection):base(collection)
    //    {
    //    }

    //    //public CollectionView(ICollectionDocument<ICollectionItem> collection, ICollectionItem doc):base(collection, doc)
    //    //{
    //    //    var _collection = doc as ICollection<ICollectionItem>;
    //    //    if (_collection!=null)
    //    //    {
    //    //        this.Items = (CollectionItemView[]) _collection.Items.ToArray();
    //    //    }
    //    //}
    //}

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

