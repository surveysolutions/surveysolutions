using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseViewFactory: IViewFactory<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>
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
                return new CompleteQuestionnaireBrowseView(input.Page, input.PageSize, count, new CompleteQuestionnaireBrowseItem[0]);
            // Perform the paged query
            IEnumerable<CompleteQuestionnaireDocument> query;

            if (!String.IsNullOrEmpty(input.ResponsibleId)) //filter result by responsible
            {
                query = documentSession.Query<CompleteQuestionnaireDocument>()
                    .Where(x => x.Responsible.Id == input.ResponsibleId)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize).ToArray();
            }
            else
            {
                query = documentSession.Query<CompleteQuestionnaireDocument>()
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize).ToArray();
            }

            var items = query
                    .Select(
                        x =>
                        new CompleteQuestionnaireBrowseItem(x.Id, x.Questionnaire.Title, x.CreationDate, x.LastEntryDate,
                                                            x.Status, x.Responsible));

                return new CompleteQuestionnaireBrowseView(
                    input.Page,
                    input.PageSize, count,
                    items);
        }
    }
}
