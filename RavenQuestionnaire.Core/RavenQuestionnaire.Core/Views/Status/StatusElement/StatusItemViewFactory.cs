using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Status.StatusElement
{
    public class StatusItemViewFactory: IViewFactory<StatusItemViewInputModel, StatusItemView>
    {
        private IDocumentSession documentSession;


        public StatusItemViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusItemView Load(StatusItemViewInputModel input)
        {
            StatusDocument doc = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QId);

            if (doc == null)
                return null; //no satelite status document 

            StatusItem item = null;

            if(input.GetDefault)
                item = doc.Statuses.FirstOrDefault(u => u.IsInitial== true);
            else
            {
                item = doc.Statuses.FirstOrDefault(u => u.PublicKey == input.PublicKey);
            }

            return item == null 
                ? null 
                : new StatusItemView(item, doc.Id, doc.QuestionnaireId);
        }
    }
}
