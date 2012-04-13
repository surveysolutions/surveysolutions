using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
   
    public class Collection:IEntity<CollectionDocument>
    {
        private CollectionDocument innerDocument;
        public string CollectionId { get { return innerDocument.Id; } }

        CollectionDocument IEntity<CollectionDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }
        
        public Collection(CollectionDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        public Collection(string title, List<CollectionItem> items)
        {
            innerDocument=new CollectionDocument(){ Name = title, Items = items};
        }

        public void UpdateText(string text)
        {
            innerDocument.Name = text;
        }

        public void ClearItems()
        {
            innerDocument.Items.Clear();
        }

        public CollectionItem AddNewItems(string _key, string _value)
        {
            CollectionItem result = new CollectionItem() { Key = _key, Value = _value};
            try
            {
                innerDocument.Items.Add(result);
                return result;
            }
            catch (Exception)
            {
               throw new ArgumentException("Not successfull!!!");
            }
        }

        public void Add(IComposite c)
        {
            innerDocument.Add(c);
        }

        public void UpdateItem(Guid publicKey, string _key, string _value)
        {
            var item = FindCollectionItem(_key);
            if (item==null)
                return;
            item.Key = _key;
            item.Value = _value;
        }

        public CollectionItem FindCollectionItem(string _key)
        {
            var item = this.innerDocument.Items.FirstOrDefault(li => li.Key == _key);
            if (item!=null)
                return (CollectionItem) item;
            return null;
        }
        
        public void Remove(IComposite c)
        {
           innerDocument.Remove(c);
        }

        public IList<CollectionItem> GetAllItems()
        {
            List<CollectionItem> result = new List<CollectionItem>();
            result.AddRange(innerDocument.Items);
            return result;
        }
    }
}





