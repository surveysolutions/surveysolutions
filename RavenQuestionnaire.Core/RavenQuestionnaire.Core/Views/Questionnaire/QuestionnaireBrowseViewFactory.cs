using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public QuestionnaireBrowseViewFactory(IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            var query = documentGroupSession.Query();
            // Adjust the model appropriately
            var count = query.Count();
            if (count == 0)
                return new QuestionnaireBrowseView(input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], "");
            // Perform the paged query
         

         /*   if (input.Orders.Count > 0)
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
                }*/

            var page = query.Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize)
                .ToList();

            // And enact this query
            var items = page
                .Select(x => new QuestionnaireBrowseItem(x.SurveyId, x.SurveyTitle, DateTime.Now, DateTime.Now))
                .ToArray();

            return new QuestionnaireBrowseView(
                input.Page,
                input.PageSize, count,
                items,
                input.Order);
        }
    }
}
