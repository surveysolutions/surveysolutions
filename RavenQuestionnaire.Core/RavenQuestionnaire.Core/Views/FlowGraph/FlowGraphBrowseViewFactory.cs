using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.FlowGraph;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class FlowGraphBrowseViewFactory : IViewFactory<FlowGraphBrowseInputModel, FlowGraphBrowseView>
    {
        private IDocumentSession documentSession;

        public FlowGraphBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public FlowGraphBrowseView Load(FlowGraphBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<QuestionnaireDocument>().Count();
            if (count == 0)
                return new FlowGraphBrowseView(input.Page, input.PageSize, count, new FlowGraphBrowseItem[0], "");

            // Perform the paged query
            IOrderedQueryable<FlowGraphDocument> query = documentSession.Query<FlowGraphDocument>();
            if (input.Orders.Count > 0)
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);

            }
            if (input.Orders.Count > 1)
                foreach (var order in input.Orders.Skip(1))
                {
                    query = order.Direction == OrderDirection.Asc
                                ? query.ThenBy(order.Field)
                                : query.ThenByDescending(order.Field);
                }

            var page = query.Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);


            // And enact this query
            var items = page
                .Select(x => new FlowGraphBrowseItem(x.Id, x.CreationDate, x.LastEntryDate))
                .ToArray();

            return new FlowGraphBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray(),
                input.Order);
        }
    }
}
