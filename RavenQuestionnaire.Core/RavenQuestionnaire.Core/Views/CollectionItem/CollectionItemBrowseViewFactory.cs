using System.Linq;
using Raven.Client;
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
                return new CollectionItemBrowseView(0,0,0);
            var doc = documentSession.Load<CollectionDocument>(input.CollectionId);
            var collectionItem = new Entities.Collection(doc).GetAllItems();
            if (collectionItem.Count != 0)
                return new CollectionItemBrowseView(input.Page, input.PageSize, input.TotalCount);
            return null;
        }
    }
}


