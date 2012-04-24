using System.Linq;
using Raven.Client;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseViewFactory:IViewFactory<CollectionItemBrowseInputModel, CollectionItemBrowseView>
    {
        private IDocumentSession documentSession;

        public CollectionItemBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CollectionItemBrowseView Load(CollectionItemBrowseInputModel input)
        {
            var count = documentSession.Query<CollectionDocument>().Count();
            if (count==0)
                return new CollectionItemBrowseView(input.CollectionId, new List<CollectionItemBrowseItem>(), input.QuestionId);
            var doc = documentSession.Load<CollectionDocument>(input.CollectionId);
            IList<Entities.SubEntities.CollectionItem> collectionItem = doc.Items;
            if (collectionItem.Count != 0)
            {
                List<CollectionItemBrowseItem> result = collectionItem.Select(item => new CollectionItemBrowseItem(item.Key, item.Value)).ToList();
                return new CollectionItemBrowseView(input.CollectionId, result, input.QuestionId);
            }
            return null;
        }
    }
}


