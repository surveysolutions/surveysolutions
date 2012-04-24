using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    public class CompleteQuestionnaireExportViewFactory : IViewFactory<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
    {
        private IDocumentSession documentSession;
        public CompleteQuestionnaireExportViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;

        }

        public CompleteQuestionnaireExportView Load(CompleteQuestionnaireExportInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<CompleteQuestionnaireDocument>().Count();
            if (count == 0)
                return new CompleteQuestionnaireExportView(input.Page, input.PageSize, count,
                                                           new CompleteQuestionnaireExportItem[0],
                                                           input.Order);

            IOrderedQueryable<CompleteQuestionnaireDocument> query;

            if (!String.IsNullOrEmpty(input.QuestionnaryId))
            {
                query =
                    (IOrderedQueryable<CompleteQuestionnaireDocument>)
                    documentSession.Query<CompleteQuestionnaireDocument>()
                        .Where(x => x.TemplateId == input.QuestionnaryId);
            }
            else
            {
                return new CompleteQuestionnaireExportView(input.Page, input.PageSize, count,
                                                          new CompleteQuestionnaireExportItem[0],
                                                          input.Order);
            }

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
                .Take(input.PageSize).ToArray();

            var items = page
                    .Select(
                        x =>
                        new CompleteQuestionnaireExportItem(x));

            return new CompleteQuestionnaireExportView(
                input.Page,
                input.PageSize, count,
                items,
                input.Order);
        }
    }
}
