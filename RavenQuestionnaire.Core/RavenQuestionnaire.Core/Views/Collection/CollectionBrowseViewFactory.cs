using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionBrowseViewFactory : IViewFactory<CollectionBrowseInputModel, CollectionBrowseView>
    {
        private IDocumentSession documentSession;

        public CollectionBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CollectionBrowseView Load(CollectionBrowseInputModel input)
        {
           var count = documentSession.Query<CollectionDocument>().Count();
            if (count == 0)
                return new CollectionBrowseView(input.Page, input.PageSize, count, new CollectionBrowseItem[0]);
            var query = documentSession.Query<CollectionDocument>().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            var items = query.Select(x => new CollectionBrowseItem(x.Id, x.Name)).ToArray();
            return new CollectionBrowseView(input.Page, input.PageSize, count, items);
        }
    }
}

