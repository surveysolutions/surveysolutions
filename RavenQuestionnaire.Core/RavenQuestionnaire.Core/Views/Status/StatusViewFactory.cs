using System.Linq;
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
            StatusDocument doc = null;

            doc = input.GetDefault 
                ? documentSession.Query<StatusDocument>().FirstOrDefault(s => s.IsInitial) 
                : documentSession.Load<StatusDocument>(input.StatusId);


            return doc == null 
                ? null 
                : new StatusView(doc.Id,doc.Title,doc.IsVisible, doc.StatusRoles, doc.QuestionnaireId);
        }
    }
}
