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
            var query = documentSession.Query<CompleteQuestionnaireDocument>()
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize).ToArray();
            
            //if (String.IsNullOrEmpty(input.ResponsibleId))
            var items = query
                .Select(x => new CompleteQuestionnaireBrowseItem(x.Id, x.Questionnaire.Title, x.CreationDate, x.LastEntryDate));
            
            return new CompleteQuestionnaireBrowseView(
                input.Page,
                input.PageSize, count,
                items);
        }
    }
}
