using System.Linq;
using Raven.Client;
using Raven.Client.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.File
{
    public class FileBrowseViewFactory : IViewFactory<FileBrowseInputModel, FileBrowseView>
    {
        private IDenormalizerStorage<FileDescription> attachments;

        public FileBrowseViewFactory(IDenormalizerStorage<FileDescription> attachments)
        {
            this.attachments = attachments;
        }

        public FileBrowseView Load(FileBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = attachments.Query().Count();
            if (count == 0)
                return new FileBrowseView(input.Page, input.PageSize, count, new FileBrowseItem[0]);

            var query = attachments.Query().Skip((input.Page - 1) * input.PageSize)
                  .Take(input.PageSize).ToList();

            // And enact this query
            var items = query
                .Select(x => new FileBrowseItem(/*x.PublicKey,*/ x.Title, x.Description, 
                   x.PublicKey))
                .ToArray();
            

            return new FileBrowseView(
                input.Page,
                input.PageSize, count,
                items);
        }
    }
}
