using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionBrowseViewFactory : IViewFactory<CollectionBrowseInputModel, CollectionBrowseView>
    {
        private readonly IDenormalizerStorage<CollectionDocument> documentItemSession;

        public CollectionBrowseViewFactory(IDenormalizerStorage<CollectionDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public CollectionBrowseView Load(CollectionBrowseInputModel input)
        {
            var count = documentItemSession.Count();
            if (count == 0)
                return new CollectionBrowseView(input.Page, input.PageSize, count, new CollectionBrowseItem[0]);

            var query = documentItemSession.Query().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            var items = query.Select(x => new CollectionBrowseItem(x.Id, x.Name)).ToArray();
            return new CollectionBrowseView(input.Page, input.PageSize, count, items);
        }
    }
}

