using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Status.Browse
{
    public class StatusBrowseViewFactory : IViewFactory<StatusBrowseInputModel, StatusBrowseView>
    {
        private IDocumentSession documentSession;


        public StatusBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusBrowseView Load(StatusBrowseInputModel input)
        {
            
            // Adjust the model appropriately
            var count = documentSession.Query<StatusDocument>().Count();
            if (count == 0)
                return new StatusBrowseView(input.Page, input.PageSize, count, new StatusBrowseItem[0], input.QId);

            // Perform the paged query
            var query = documentSession.Query<StatusDocument>()
                .Where(x => x.QuestionnaireId == input.QId)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);


            // And enact this query
            var items = query
                .Select(x => new StatusBrowseItem(x.Id, string.Empty, x.QuestionnaireId))
                .ToArray();

            return new StatusBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray(),
                input.QId);
        }
    }
}
