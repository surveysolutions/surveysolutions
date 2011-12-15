using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusViewFactory: IViewFactory<StatusViewInputModel, StatusView>
    {
        private IDocumentSession documentSession;


        public StatusViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusView Load(StatusViewInputModel input)
        {

            var doc = documentSession.Load<StatusDocument>(input.StatusId);
            /*  var questions =
                  documentSession.Query<QuestionDocument, QuestionnaireContainingQuestions>().Where(
                      question => question.QuestionnaireId.Equals(doc.Id));*/

            return new StatusView(doc.Id,doc.Title,doc.IsVisible, doc.StatusRoles);
        
        }
    }
}
