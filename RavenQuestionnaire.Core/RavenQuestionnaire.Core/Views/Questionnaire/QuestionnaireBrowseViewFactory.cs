using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        private IDocumentSession documentSession;

        public QuestionnaireBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
           var count = documentSession.Query<QuestionnaireDocument>().Count();
            if (count == 0)
                return new QuestionnaireBrowseView(input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0]);
            // Perform the paged query
            var query = documentSession.Query<QuestionnaireDocument>()
                    .Skip((input.Page-1) * input.PageSize)
                    .Take(input.PageSize);

           
            // And enact this query
            var items = query
                .Select(x => new QuestionnaireBrowseItem(x.Id, x.Title, x.CreationDate, x.LastEntryDate))
                .ToArray();

            return new QuestionnaireBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray());
        }
    }
}
