using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseViewFactory : IViewFactory<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CompleteQuestionnaireBrowseView Load(CompleteQuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<CompleteQuestionnaireDocument>().Count();
            if (count == 0)
                return new CompleteQuestionnaireBrowseView(input.Page, input.PageSize, count,
                                                           new CompleteQuestionnaireBrowseItem[0],
                                                           input.Order);

            IOrderedQueryable<CompleteQuestionnaireDocument> query;

            if (!String.IsNullOrEmpty(input.ResponsibleId)) //filter result by responsible
            {
                query =
                    (IOrderedQueryable<CompleteQuestionnaireDocument>)
                    documentSession.Query<CompleteQuestionnaireDocument>()
                        .Where(x => x.Responsible.Id == input.ResponsibleId);
            }
            else
            {
                query = documentSession.Query<CompleteQuestionnaireDocument>();
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
                        new CompleteQuestionnaireBrowseItem(x.Id, x.Questionnaire.Title, x.CreationDate, x.LastEntryDate,
                                                            x.Status, x.Responsible));

            return new CompleteQuestionnaireBrowseView(
                input.Page,
                input.PageSize, count,
                items,
                input.Order);
        }
    }
}
