using System.Linq;
using Raven.Client;
using Raven.Client.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseViewFactory : IViewFactory<FileBrowseInputModel, FileBrowseView>
    {
        private IDocumentSession documentSession;

        public FileBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public FileBrowseView Load(FileBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<FileDocument>().Count();
            if (count == 0)
                return new FileBrowseView(input.Page, input.PageSize, count, new FileBrowseItem[0]);

            var query = documentSession.Query<FileDocument>().Skip((input.Page - 1) * input.PageSize)
                  .Take(input.PageSize);

            // And enact this query
            var items = query
                .Select(x => new FileBrowseItem(x.Id, x.Title, x.Description, x.CreationDate, 
                   x.Filename, x.Width, x.Height,
                   x.Thumbnail, x.ThumbnailWidth, x.ThumbnailHeight))
                .ToArray();

            return new FileBrowseView(
                input.Page,
                input.PageSize, count,
                items);
        }
    }
}
