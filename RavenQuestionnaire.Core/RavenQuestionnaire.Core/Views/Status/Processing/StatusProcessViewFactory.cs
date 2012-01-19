using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessViewFactory: IViewFactory<StatusProcessViewInputModel, StatusProcessView>
    {
        private IDocumentSession documentSession;


        public StatusProcessViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusProcessView Load(StatusProcessViewInputModel input)
        {
            StatusDocument doc = documentSession.Load<StatusDocument>(input.StatusId);

            return doc == null ? null
                : new StatusProcessView(doc.Id, doc.Title, doc.IsVisible, doc.FlowRules, 
                    doc.DefaultIfNoConditions, doc.QuestionnaireId);
        }
    }
}
