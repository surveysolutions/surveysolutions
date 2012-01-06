using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportBrowseViewFactory : IViewFactory<ReportBrowseInputModel, ReportBrowseView>
    {
        private IDocumentSession documentSession;


        public ReportBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public ReportBrowseView Load(ReportBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<ReportDocument>().Count();
            if (count == 0)
                return new ReportBrowseView(input.Page, input.PageSize, count, new ReportBrowseItem[0]);
            // Perform the paged query
            var query = documentSession.Query<ReportDocument>()
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);
            
            // And enact this query
            var items = query
                .Select(x => new ReportBrowseItem(x.Id, x.Title, x.Description))
                .ToArray();

            return new ReportBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray());
        }
    }
}
